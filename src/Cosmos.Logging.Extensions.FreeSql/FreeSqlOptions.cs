﻿using System;
using System.Collections.Generic;
using Cosmos.Logging.Configurations;
using Cosmos.Logging.Core;
using Cosmos.Logging.Events;
using Cosmos.Logging.Extensions.FreeSql.Core;
using FreeSql.Aop;

// ReSharper disable once CheckNamespace
namespace Cosmos.Logging
{
    public class FreeSqlOptions : ILoggingSinkOptions<FreeSqlOptions>, ILoggingSinkOptions
    {
        public FreeSqlOptions() { }

        public string Key => Constants.SinkKey;

        #region Append log minimum level

        internal readonly Dictionary<string, LogEventLevel> InternalNavigatorLogEventLevels = new Dictionary<string, LogEventLevel>();

        internal LogEventLevel? MinimumLevel { get; set; }

        public FreeSqlOptions UseMinimumLevelForType<T>(LogEventLevel level) => UseMinimumLevelForType(typeof(T), level);

        public FreeSqlOptions UseMinimumLevelForType(Type type, LogEventLevel level)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            var typeName = TypeNameHelper.GetTypeDisplayName(type);
            if (InternalNavigatorLogEventLevels.ContainsKey(typeName))
            {
                InternalNavigatorLogEventLevels[typeName] = level;
            }
            else
            {
                InternalNavigatorLogEventLevels.Add(typeName, level);
            }

            return this;
        }

        public FreeSqlOptions UseMinimumLevelForCategoryName<T>(LogEventLevel level) => UseMinimumLevelForCategoryName(typeof(T), level);

        public FreeSqlOptions UseMinimumLevelForCategoryName(Type type, LogEventLevel level)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            var @namespace = type.Namespace;
            return UseMinimumLevelForCategoryName(@namespace, level);
        }

        public FreeSqlOptions UseMinimumLevelForCategoryName(string categoryName, LogEventLevel level)
        {
            if (string.IsNullOrWhiteSpace(categoryName)) throw new ArgumentNullException(nameof(categoryName));
            categoryName = $"{categoryName}.*";
            if (InternalNavigatorLogEventLevels.ContainsKey(categoryName))
            {
                InternalNavigatorLogEventLevels[categoryName] = level;
            }
            else
            {
                InternalNavigatorLogEventLevels.Add(categoryName, level);
            }

            return this;
        }

        public FreeSqlOptions UseMinimumLevel(LogEventLevel? level)
        {
            MinimumLevel = level;
            return this;
        }

        #endregion

        #region Append log level alias

        internal readonly Dictionary<string, LogEventLevel> InternalAliases = new Dictionary<string, LogEventLevel>();

        public FreeSqlOptions UseAlias(string alias, LogEventLevel level)
        {
            if (string.IsNullOrWhiteSpace(alias)) return this;
            if (InternalAliases.ContainsKey(alias))
            {
                InternalAliases[alias] = level;
            }
            else
            {
                InternalAliases.Add(alias, level);
            }

            return this;
        }

        #endregion

        #region Append Interceptor

        internal Action<Exception, CurdAfterEventArgs> ErrorInterceptorAction { get; set; }

        internal Action<CurdBeforeEventArgs> ExecutingInterceptorAction { get; set; }

        internal Action<CurdAfterEventArgs> ExecutedInterceptorAction { get; set; }

        public FreeSqlOptions AddExecutingInterceptor(Action<CurdBeforeEventArgs> executingInterceptor)
        {
            ExecutingInterceptorAction += executingInterceptor ?? throw new ArgumentNullException(nameof(executingInterceptor));
            return this;
        }

        public FreeSqlOptions AddExecutedInterceptor(Action<CurdAfterEventArgs> executedInterceptor)
        {
            ExecutedInterceptorAction += executedInterceptor ?? throw new ArgumentNullException(nameof(executedInterceptor));
            return this;
        }

        public FreeSqlOptions AddErrorInterceptor(Action<Exception, CurdAfterEventArgs> errorInterceptor)
        {
            ErrorInterceptorAction += errorInterceptor ?? throw new ArgumentNullException(nameof(errorInterceptor));
            return this;
        }

        #endregion

        #region Appeng filter

        internal Func<string, LogEventLevel, bool> Filter { get; set; }

        public FreeSqlOptions UseFilter(Func<string, LogEventLevel, bool> filter)
        {
            if (filter == null) throw new ArgumentNullException(nameof(filter));

            var temp = Filter;
            Filter = (s, l) => (temp?.Invoke(s, l) ?? true) && filter(s, l);

            return this;
        }

        #endregion

        #region Append output

        private readonly RendingConfiguration _renderingOptions = new RendingConfiguration();

        public FreeSqlOptions EnableDisplayCallerInfo(bool? displayingCallerInfoEnabled)
        {
            _renderingOptions.DisplayingCallerInfoEnabled = displayingCallerInfoEnabled;
            return this;
        }

        public FreeSqlOptions EnableDisplayEventIdInfo(bool? displayingEventIdInfoEnabled)
        {
            _renderingOptions.DisplayingEventIdInfoEnabled = displayingEventIdInfoEnabled;
            return this;
        }

        public FreeSqlOptions EnableDisplayingNewLineEom(bool? displayingNewLineEomEnabled)
        {
            _renderingOptions.DisplayingNewLineEomEnabled = displayingNewLineEomEnabled;
            return this;
        }

        public RendingConfiguration GetRenderingOptions() => _renderingOptions;

        #endregion

    }
}