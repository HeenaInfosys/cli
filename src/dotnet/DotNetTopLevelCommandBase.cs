﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.DotNet.Cli.CommandLine;
using Microsoft.DotNet.Cli.Utils;
using Microsoft.DotNet.Tools;

namespace Microsoft.DotNet.Cli
{
    public abstract class DotNetTopLevelCommandBase
    {
        protected abstract string CommandName { get; }
        protected abstract string FullCommandNameLocalized { get; }
        protected abstract string ArgumentName { get; }
        protected abstract string ArgumentDescriptionLocalized { get; }
        protected ParseResult ParseResult { get; private set; }

        internal abstract Dictionary<string, Func<AppliedOption, CommandBase>> SubCommands { get; }

        public int RunCommand(string[] args)
        {
            DebugHelper.HandleDebugSwitch(ref args);

            var parser = Parser.Instance;

            ParseResult = parser.ParseFrom($"dotnet {CommandName}", args);

            ParseResult.ShowHelpIfRequested();

            var subcommandName = ParseResult.Command().Name;

            try
            {
                var create = SubCommands[subcommandName];

                var command = create(ParseResult["dotnet"][CommandName]);

                return command.Execute();
            }
            catch (KeyNotFoundException)
            {
                throw new GracefulException(CommonLocalizableStrings.RequiredCommandNotPassed);
            }
            catch (GracefulException e)
            {
                Reporter.Error.WriteLine(e.Message.Red());
                ParseResult.ShowHelp();
                return 1;
            }
        }
    }
}