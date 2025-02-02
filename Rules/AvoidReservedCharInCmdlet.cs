﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation.Language;
using Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic;
#if !CORECLR
using System.ComponentModel.Composition;
#endif
using System.Globalization;
using System.Management.Automation;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules
{

    /// <summary>
    /// AvoidReservedCharInCmdlet: Analyzes script to check for reserved characters in cmdlet names.
    /// </summary>
#if !CORECLR
[Export(typeof(IScriptRule))]
#endif
    public class AvoidReservedCharInCmdlet : IScriptRule
    {
        /// <summary>
        /// Analyze ast to check that all the cmdlet does not use reserved char
        /// </summary>
        /// <param name="ast"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public IEnumerable<DiagnosticRecord> AnalyzeScript(Ast ast, string fileName)
        {
            if (ast == null) throw new ArgumentNullException(Strings.NullAstErrorMessage);

            IEnumerable<Ast> funcAsts = ast.FindAll(item => item is FunctionDefinitionAst, true);

            string reservedChars = Strings.ReserverCmdletChars;
            HashSet<string> exportedFunction = Helper.Instance.GetExportedFunction(ast);

            foreach (FunctionDefinitionAst funcAst in funcAsts)
            {
                if (funcAst.Body != null && funcAst.Body.ParamBlock != null && funcAst.Body.ParamBlock.Attributes != null)
                {
                    // only raise this rule for function with cmdletbinding
                    if (!funcAst.Body.ParamBlock.Attributes.Any(attr => attr.TypeName.GetReflectionType() == typeof(CmdletBindingAttribute)))
                    {
                        continue;
                    }

                    string funcName = Helper.Instance.FunctionNameWithoutScope(funcAst.Name);

                    // only raise if the function is exported
                    if (funcName != null && funcName.Intersect(reservedChars).Count() > 0 && (exportedFunction.Contains(funcAst.Name) || exportedFunction.Contains(funcName)))
                    {
                        yield return new DiagnosticRecord(string.Format(CultureInfo.CurrentCulture, Strings.ReservedCmdletCharError, funcAst.Name),
                            funcAst.Extent, GetName(), DiagnosticSeverity.Unknown, GetCategory(), fileName);
                    }                
                }
            }            
        }

        /// <summary>
        /// GetName: Retrieves the name of this rule.
        /// </summary>
        /// <returns>The name of this rule</returns>
        public string GetName() {
            return string.Format(CultureInfo.CurrentCulture, Strings.NameSpaceFormat, GetSourceName(), Strings.ReservedCmdletCharName);
        }

        /// <summary>
        /// GetCommonName: Retrieves the common name of this rule.
        /// </summary>
        /// <returns>The common name of this rule</returns>
        public string GetCommonName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.ReservedCmdletCharCommonName);
        }

        /// <summary>
        /// GetDescription: Retrieves the description of this rule.
        /// </summary>
        /// <returns>The description of this rule</returns>
        public string GetDescription() {
            return string.Format(CultureInfo.CurrentCulture, Strings.ReservedCmdletCharDescription);
        }

        /// <summary>
        /// GetSourceType: Retrieves the type of the rule: builtin, managed or module.
        /// </summary>
        public SourceType GetSourceType()
        {
            return SourceType.Builtin;
        }

        /// <summary>
        /// GetSeverity: Retrieves the severity of the rule: critical, high, medium or information.
        /// </summary>
        /// <returns></returns>
        public RuleSeverity GetSeverity()
        {
            return RuleSeverity.Medium;
        }

        /// <summary>
        /// GetCategory: Retrieves the category of the rule: InputValidation, OutputEncoding, AuthenticationandPasswordManagement, SessionManagement, AccessControl, CryptographicPractices, ErrorHandlingandLogging, DataProtection, CommunicationSecurity, SystemConfiguration, DatabaseSecurity, FileManagement, MemoryManagement, GeneralCodingPractices.
        /// </summary>
        /// <returns></returns>
        public RuleCategory GetCategory()
        {
            return RuleCategory.Unknown;
        }
        

        /// <summary>
        /// GetSourceName: Retrieves the module/assembly name the rule is from.
        /// </summary>
        public string GetSourceName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.SourceName);
        }
    }

}




