// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic
{
    /// <summary>
    /// Represents the severity of a PSScriptAnalyzer rule
    /// </summary>
    public enum RuleSeverity : uint
    {
        /// <summary>
        /// Information: This warning is trivial, but may be useful. They are recommended by PowerShell best practice.
        /// </summary>
        Information = 0,

        /// <summary>
        /// MEDIUM: This warning may cause a problem or does not follow PowerShell's recommended guidelines.
        /// </summary>
        Medium = 1,

        /// <summary>
        /// HIGH: This warning is likely to cause a problem or does not follow PowerShell's required guidelines.
        /// </summary>
        High = 2,

        /// <summary>
        /// CRITICAL: This warning is likely to cause major problems and does not follow PowerShell's required guidelines.
        /// </summary>
        Critical = 3,

        /// <summary>
        /// PARSEERROR: This diagnostic is caused by an actual parsing error, and is generated only by the engine.
        /// </summary>
        ParseError    = 4,
    };
}
