// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Management.Automation.Language;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.PowerShell.CrossCompatibility.Query;
using Microsoft.PowerShell.CrossCompatibility.Retrieval;
using Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules
{
    /// <summary>
    /// Convenient work-sharing base class for PSSA rules that require compatiblity profiles.
    /// </summary>
    public abstract class CompatibilityRule : ConfigurableRule
    {
        // The name of the directory where compatibility profiles are looked for by default.
        private const string PROFILE_DIR_NAME = "compatibility_profiles";

        // The full path of the directory where compatiblity profiles are looked for by default.
        private static readonly string s_defaultProfileDirPath;

        // Memoized path to the module root of PSScriptAnalyzer.
        private static readonly Lazy<string> s_moduleRootDirPath;

        // A regex to differentiate profiles without extensions (but with dots in the names)
        private static readonly Regex s_falseProfileExtensionPattern = new Regex(
            "\\d+_(core|framework)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        static CompatibilityRule()
        {
            s_moduleRootDirPath = new Lazy<string>(() => GetModuleRootDirPath());
            s_defaultProfileDirPath = Path.Combine(s_moduleRootDirPath.Value, PROFILE_DIR_NAME);
        }

        private readonly CompatibilityProfileLoader _profileLoader;

        private DirectoryInfo _profileDir;

        /// <summary>
        /// Create a new compatibility rule with the default profile loading directory.
        /// </summary>
        protected CompatibilityRule()
            : this(s_defaultProfileDirPath)
        {
        }

        /// <summary>
        /// Create a new compatiblity rule with the configured profile loading directory.
        /// </summary>
        /// <param name="profileDirPath">The directory to search for profiles in by default.</param>
        protected CompatibilityRule(string profileDirPath)
        {
            _profileDir = new DirectoryInfo(profileDirPath);
            _profileLoader = CompatibilityProfileLoader.StaticInstance;
        }

        /// <summary>
        /// The profile names or filenames of profiles to load from the profile directory,
        /// as well as absolute paths to other profiles.
        /// </summary>
        [ConfigurableRuleProperty(defaultValue: new string[] {})]
        public string[] TargetProfiles { get; set; }

        /// <summary>
        /// Override property to set the directory where profiles
        /// given by name will be looked for and loaded from.
        /// </summary>
        /// <remarks>
        /// This property should default to s_defaultProfileDirPath,
        /// but rather than use a default property, we are required to
        /// specify a value in the attribute, which only allows constant strings.
        /// We should fix the ConfigurableRule logic to change this.
        /// </remarks>
        [ConfigurableRuleProperty(defaultValue: "")]
        public string ProfileDirPath { get; set; }

        /// <summary>
        /// The severity of diagnostics generated by this rule.
        /// </summary>
        public virtual DiagnosticSeverity DiagnosticSeverity => DiagnosticSeverity.Unknown;

        /// <summary>
        /// Method to be override to generate the visitor for AST analysis.
        /// The pattern suggested is to simply override this method for visitor creation
        /// and let this parent class hook up the AST analysis call.
        /// </summary>
        /// <param name="fileName">The full path of the script being analyzed.</param>
        /// <returns>An AST visitor that generates diagnostics for PowerShell compatibility.</returns>
        protected abstract CompatibilityVisitor CreateVisitor(string fileName);

        /// <summary>
        /// Analyze a given ast and provide any warning diagnostics about possible compatiblity issues.
        /// </summary>
        /// <param name="ast">The PowerShell AST to analyze.</param>
        /// <param name="fileName">The file path of the PowerShell script being analyzed.</param>
        /// <returns>Any diagnostics detailing compatibility issues with the given AST.</returns>
        public override IEnumerable<DiagnosticRecord> AnalyzeScript(Ast ast, string fileName)
        {
            CompatibilityVisitor compatibilityVisitor = CreateVisitor(fileName);
            ast.Visit(compatibilityVisitor);
            return compatibilityVisitor.GetDiagnosticRecords();
        }

        /// <summary>
        /// Gets the severity of this rule.
        /// </summary>
        public override RuleSeverity GetSeverity()
        {
            return RuleSeverity.Medium;
        }

        /// <summary>
        /// Gets the source type of this rule.
        /// </summary>
        public override SourceType GetSourceType()
        {
            return SourceType.Builtin;
        }

        /// <summary>
        /// Gets the name of the source of this rule.
        /// </summary>
        public override string GetSourceName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.SourceName);
        }

        /// <summary>
        /// Overrides the ConfigurableRule method to allow setting of a non-constant
        /// profile directory.
        /// </summary>
        /// <param name="paramValueMap">The configuration values passed in to set the rule.</param>
        public override void ConfigureRule(IDictionary<string, object> paramValueMap)
        {
            base.ConfigureRule(paramValueMap);

            // Override the ProfileDirPath if one is given
            if (string.IsNullOrEmpty(ProfileDirPath))
            {
                return;
            }

            _profileDir = new DirectoryInfo(ProfileDirPath.TrimEnd('/', '\\'));
        }

        /// <summary>
        /// Call this method to load the compatibilty profiles configured for this rule.
        /// </summary>
        /// <returns>The any profile and a list of target profiles for this rule.</returns>
        protected IEnumerable<CompatibilityProfileData> LoadCompatibilityProfiles(out CompatibilityProfileData unionProfile)
        {
            if (TargetProfiles == null)
            {
                throw new InvalidOperationException($"{nameof(TargetProfiles)} cannot be null");
            }

            if (TargetProfiles.Length == 0)
            {
                throw new InvalidOperationException($"{nameof(TargetProfiles)} cannot be empty");
            }

            return _profileLoader.GetProfilesWithUnion(_profileDir, TargetProfiles.Select(path => NormalizeProfileNameToAbsolutePath(path)), out unionProfile);
        }

        private string NormalizeProfileNameToAbsolutePath(string profileName)
        {
            // Reject null or empty paths
            if (string.IsNullOrEmpty(profileName))
            {
                throw new ArgumentException($"{nameof(profileName)} cannot be null or empty");
            }

            // Accept absolute paths verbatim. There may be issues with paths like "/here" in Windows
            if (Path.IsPathRooted(profileName))
            {
                return profileName;
            }

            // Reject relative paths
            if (profileName.Contains("\\")
                || profileName.Contains("/")
                || profileName.Equals(".")
                || profileName.Equals(".."))
            {
                throw new ArgumentException($"Compatibility profile specified as '{profileName}'. Compatibility profiles cannot be specified by relative path.");
            }

            // Profiles might be given by pure name, in which case tack ".json" onto the end
            string extension = Path.GetExtension(profileName);
            if (string.IsNullOrEmpty(extension) || s_falseProfileExtensionPattern.IsMatch(extension))
            {
                profileName = profileName + ".json";
            }

            // Names get looked for in the known profile directory
            return Path.Combine(_profileDir.FullName, profileName);
        }

        /// <summary>
        /// Get the path of PSScriptAnalyzer on the file system.
        /// </summary>
        /// <returns>The absolute path of the PSScriptAnalyzer module root.</returns>
        private static string GetModuleRootDirPath()
        {
            // Start from the directory containing the Rules DLL,
            // which may be in the module root or in a child directory (ex: coreclr)
            string asmDirLocation = Path.GetDirectoryName(typeof(CompatibilityRule).Assembly.Location);

            // Search down the directory structure from the assembly location looking for the module root
            // We may be in a versioned directory ("PSScriptAnalyzer/1.18.0" vs "PSScriptAnalyzer"), so can't search that way
            // Instead we look for the PSSA module manifest
            const string manifestName = "PSScriptAnalyzer.psd1";

            // Look for PSScriptAnalyzer.psd1 next to the Rules DLL
            string manifestPath = Path.Combine(asmDirLocation, manifestName);
            if (File.Exists(manifestPath))
            {
                return asmDirLocation;
            }

            // Look for PSScriptAnalyzer.psd1 in the directory above the Rules DLL
            string dirUpOneLevel = Path.GetDirectoryName(asmDirLocation);
            manifestPath = Path.Combine(dirUpOneLevel, manifestName);
            if (File.Exists(manifestPath))
            {
                return dirUpOneLevel;
            }

            // Unable to find the root of the module where it should be, so we give up
            throw new FileNotFoundException("Unable to find the PSScriptAnalyzer module root");
        }
    }

    /// <summary>
    /// Base class for an AST visitor that generates diagnostics about compatibility with
    /// target PowerShell runtimes, to be used in conjunction with the CompatibilityRule class.
    /// </summary>
    public abstract class CompatibilityVisitor : AstVisitor
    {
        public abstract IEnumerable<DiagnosticRecord> GetDiagnosticRecords();
    }

    /// <summary>
    /// Base class for compatibility diagnostics with some assumptions baked in about
    /// diagnostic severity and rule ID.
    /// </summary>
    public abstract class CompatibilityDiagnostic : DiagnosticRecord
    {
        protected CompatibilityDiagnostic(
            string message,
            IScriptExtent extent,
            string ruleName,
            string ruleId,
            string analyzedFileName,
            IEnumerable<CorrectionExtent> suggestedCorrections)
            : base(
                message,
                extent,
                ruleName,
                DiagnosticSeverity.Unknown,
                RuleCategory.Unknown,
                analyzedFileName,
                ruleId,
                suggestedCorrections: suggestedCorrections)
        {
        }
    }
}
