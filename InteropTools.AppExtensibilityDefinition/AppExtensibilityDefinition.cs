﻿using AppPlugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Windows.ApplicationModel.AppService;

namespace InteropTools.AppExtensibilityDefinition
{
    public abstract class AppExtensibilityDefinition : AbstractPlugin<string, string, double>//TransfareOptions
    {

        public const String PLUGIN_NAME = "InteropTools.External.AppExtensibility";

        protected sealed override Task<string> Execute(AppServiceConnection sender, string input, IProgress<double> progress, CancellationToken cancelToken)//, TransfareOptions options
        {
            /*if (options.OptionsIdentifyer != GetOptionsGuid())
                throw new ArgumentException("Option Not generated by this Plugin", nameof(options));*/
            return ExecuteAsync(sender, input, progress, cancelToken); //, options.Options;
        }

        /*protected sealed override async Task<TransfareOptions> GetDefaultOptionsAsync()
        => new TransfareOptions() { Options = await GetOptions() };*/

        protected abstract Task<string> ExecuteAsync(AppServiceConnection sender, string input, IProgress<double> progress, CancellationToken cancelToken); //, Options options

        protected abstract Task<Options> GetOptions();
        protected abstract Guid GetOptionsGuid();
    }
}
