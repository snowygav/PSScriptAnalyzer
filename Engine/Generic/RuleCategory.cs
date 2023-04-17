// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic
{
    /// <summary>
    /// Represents the category of a PSScriptAnalyzer rule
    /// </summary>
    public enum RuleCategory : uint
    {
        /// <summary>
        /// Input Validation
        /// </summary>
        InputValidation = 0,

        /// <summary>
        /// Output Encoding
        /// </summary>
        OutputEncoding = 1,

        /// <summary>
        /// Authentication and Password Management
        /// </summary>
        AuthenticationAndPasswordManagement = 2,

        /// <summary>
        /// Session Manangment
        /// </summary>
        SessionManagement = 3,

        /// <summary>
        /// Access Control
        /// </summary>
        AccessControl = 4,

        /// <summary>
        /// 
        /// </summary>
        CryptographicPractices = 5,

        /// <summary>
        /// 
        /// </summary>
        ErrorHandlingandLogging = 6,

        /// <summary>
        /// 
        /// </summary>
        DataProtection = 7,

        /// <summary>
        /// 
        /// </summary>
        CommunicationSecurity = 8,

        /// <summary>
        /// System Configuration
        /// </summary>
        SystemConfiguration = 9,

        /// <summary>
        /// Database Security
        /// </summary>
        DatabaseSecurity = 10,

        /// <summary>
        /// File Management
        /// </summary>
        FileManagement = 11,

        /// <summary>
        /// Memory Management
        /// </summary>
        MemoryManagement = 12,

        /// <summary>
        /// General Coding Practices
        /// </summary>
        GeneralCodingPractices = 13,
    };
}
