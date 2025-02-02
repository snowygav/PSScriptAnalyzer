// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
#if !CORECLR
using System.ComponentModel.Composition;
#endif
using System.Globalization;
using System.Management.Automation.Language;
using Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules
{
    /// <summary>
    /// AvoidLongLines: Checks for lines longer than 120 characters
    /// </summary>
#if !CORECLR
    [Export(typeof(IScriptRule))]
#endif
    public class AvoidLongLines : ConfigurableRule
    {
        /// <summary>
        /// Construct an object of AvoidLongLines type.
        /// </summary>
        public AvoidLongLines()
        { }

        [ConfigurableRuleProperty(defaultValue: 120)]
        public int MaximumLineLength { get; set; }

        private readonly string[] s_lineSeparators = new[] { "\r\n", "\n" };

        /// <summary>
        /// Analyzes the given ast to find violations.
        /// </summary>
        /// <param name="ast">AST to be analyzed. This should be non-null</param>
        /// <param name="fileName">Name of file that corresponds to the input AST.</param>
        /// <returns>A an enumerable type containing the violations</returns>
        public override IEnumerable<DiagnosticRecord> AnalyzeScript(Ast ast, string fileName)
        {
            if (ast == null)
            {
                throw new ArgumentNullException(nameof(ast));
            }

            var diagnosticRecords = new List<DiagnosticRecord>();

            string[] lines = ast.Extent.Text.Split(s_lineSeparators, StringSplitOptions.None);

            for (int lineNumber = 0; lineNumber < lines.Length; lineNumber++)
            {
                string line = lines[lineNumber];

                if (line.Length <= MaximumLineLength)
                {
                    continue;
                }

                int startLine = lineNumber + 1;
                int endLine = startLine;
                int startColumn = 1;
                int endColumn = line.Length;

                var violationExtent = new ScriptExtent(
                    new ScriptPosition(
                        ast.Extent.File,
                        startLine,
                        startColumn,
                        line
                    ),
                    new ScriptPosition(
                        ast.Extent.File,
                        endLine,
                        endColumn,
                        line
                    ));

                var record = new DiagnosticRecord(
                        String.Format(CultureInfo.CurrentCulture, 
                            String.Format(Strings.AvoidLongLinesError, MaximumLineLength)),
                        violationExtent,
                        GetName(),
                        GetDiagnosticSeverity(),
                        GetCategory(), 
                        ast.Extent.File,
                        null
                    );
                diagnosticRecords.Add(record);
            }

            return diagnosticRecords;
        }

        /// <summary>
        /// Retrieves the common name of this rule.
        /// </summary>
        public override string GetCommonName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.AvoidLongLinesCommonName);
        }

        /// <summary>
        /// Retrieves the description of this rule.
        /// </summary>
        public override string GetDescription()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.AvoidLongLinesDescription);
        }

        /// <summary>
        /// Retrieves the name of this rule.
        /// </summary>
        public override string GetName()
        {
            return string.Format(
                CultureInfo.CurrentCulture,
                Strings.NameSpaceFormat,
                GetSourceName(),
                Strings.AvoidLongLinesName);
        }

        /// <summary>
        /// Retrieves the severity of the rule: critical, high, medium or information.
        /// </summary>
        public override RuleSeverity GetSeverity()
        {
            return RuleSeverity.Medium;
        }

        /// <summary>
        /// GetCategory: Retrieves the category of the rule: InputValidation, OutputEncoding, AuthenticationandPasswordManagement, SessionManagement, AccessControl, CryptographicPractices, ErrorHandlingandLogging, DataProtection, CommunicationSecurity, SystemConfiguration, DatabaseSecurity, FileManagement, MemoryManagement, GeneralCodingPractices.
        /// </summary>
        /// <returns></returns>
        public override RuleCategory GetCategory()
        {
            return RuleCategory.Unknown;
        }
        

        /// <summary>
        /// Gets the severity of the returned diagnostic record: critical, high, medium or information.
        /// </summary>
        /// <returns></returns>
        private DiagnosticSeverity GetDiagnosticSeverity()
        {
            return DiagnosticSeverity.Unknown;
        }

        /// <summary>
        /// Retrieves the name of the module/assembly the rule is from.
        /// </summary>
        public override string GetSourceName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.SourceName);
        }

        /// <summary>
        /// Retrieves the type of the rule, Builtin, Managed or Module.
        /// </summary>
        public override SourceType GetSourceType()
        {
            return SourceType.Builtin;
        }
    }
}
