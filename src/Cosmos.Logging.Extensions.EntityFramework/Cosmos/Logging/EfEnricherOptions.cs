﻿using System;
using System.Collections.Generic;
using System.Data.Common;
using Cosmos.Logging.Configurations;
using Cosmos.Logging.Core;
using Cosmos.Logging.Events;
using Cosmos.Logging.Extensions.EntityFramework.Core;

namespace Cosmos.Logging {
    /// <summary>
    /// Cosmos Logging EntityFramework Enricher optons
    /// </summary>
    public class EfEnricherOptions : ILoggingSinkOptions<EfEnricherOptions>, ILoggingSinkOptions {

        /// <inheritdoc />
        public string Key => Constants.SinkKey;

        #region Append log minimum level

        internal readonly Dictionary<string, LogEventLevel> InternalNavigatorLogEventLevels = new Dictionary<string, LogEventLevel>();

        internal LogEventLevel? MinimumLevel { get; set; }

        /// <inheritdoc />
        public EfEnricherOptions UseMinimumLevelForType<T>(LogEventLevel level) => UseMinimumLevelForType(typeof(T), level);

        /// <inheritdoc />
        public EfEnricherOptions UseMinimumLevelForType(Type type, LogEventLevel level) {
            if (type is null) throw new ArgumentNullException(nameof(type));
            var typeName = TypeNameHelper.GetTypeDisplayName(type);
            if (InternalNavigatorLogEventLevels.ContainsKey(typeName)) {
                InternalNavigatorLogEventLevels[typeName] = level;
            }
            else {
                InternalNavigatorLogEventLevels.Add(typeName, level);
            }

            return this;
        }

        /// <inheritdoc />
        public EfEnricherOptions UseMinimumLevelForCategoryName<T>(LogEventLevel level) => UseMinimumLevelForCategoryName(typeof(T), level);

        /// <inheritdoc />
        public EfEnricherOptions UseMinimumLevelForCategoryName(Type type, LogEventLevel level) {
            if (type is null) throw new ArgumentNullException(nameof(type));
            var @namespace = type.Namespace;
            return UseMinimumLevelForCategoryName(@namespace, level);
        }

        /// <inheritdoc />
        public EfEnricherOptions UseMinimumLevelForCategoryName(string @namespace, LogEventLevel level) {
            if (string.IsNullOrWhiteSpace(@namespace)) throw new ArgumentNullException(nameof(@namespace));
            @namespace = $"{@namespace}.*";
            if (InternalNavigatorLogEventLevels.ContainsKey(@namespace)) {
                InternalNavigatorLogEventLevels[@namespace] = level;
            }
            else {
                InternalNavigatorLogEventLevels.Add(@namespace, level);
            }

            return this;
        }

        /// <inheritdoc />
        public EfEnricherOptions UseMinimumLevel(LogEventLevel? level) {
            MinimumLevel = level;
            return this;
        }

        #endregion

        #region Append log level alias

        internal readonly Dictionary<string, LogEventLevel> InternalAliases = new Dictionary<string, LogEventLevel>();

        /// <inheritdoc />
        public EfEnricherOptions UseAlias(string alias, LogEventLevel level) {
            if (string.IsNullOrWhiteSpace(alias)) return this;
            if (InternalAliases.ContainsKey(alias)) {
                InternalAliases[alias] = level;
            }
            else {
                InternalAliases.Add(alias, level);
            }

            return this;
        }

        #endregion

        #region Append Interceptor

        internal Func<string, object> SimgleLoggingAction { get; set; }

        /// <summary>
        /// Add simple logging interceptor
        /// </summary>
        /// <param name="simpleLoggingInterceptor"></param>
        /// <returns></returns>
        public EfEnricherOptions AddSimpleLoggingInterceptor(Func<string, object> simpleLoggingInterceptor) {
            SimgleLoggingAction += simpleLoggingInterceptor;
            return this;
        }


        internal Func<Exception, string, DbParameterCollection, DateTime, DateTime, object> ErrorInterceptorAction { get; set; }
        internal Action<string, DbParameterCollection, DateTime> ExecutingInterceptorAction { get; set; }
        internal Func<string, DbParameterCollection, DateTime, DateTime, object> ExecutedInterceptorAction { get; set; }
        internal Func<string, DbParameterCollection, DateTime, DateTime, object> LongTimeExecutedInterceptorAction { get; set; }

        /// <summary>
        /// Add executing interceptor
        /// </summary>
        /// <param name="executingInterceptor"></param>
        /// <returns></returns>
        public EfEnricherOptions AddExecutingInterceptor(Action<string, DbParameterCollection, DateTime> executingInterceptor) {
            ExecutingInterceptorAction += executingInterceptor ?? throw new ArgumentNullException(nameof(executingInterceptor));
            return this;
        }

        /// <summary>
        /// Add executed interceptor
        /// </summary>
        /// <param name="executedInterceptor"></param>
        /// <returns></returns>
        public EfEnricherOptions AddExecutedInterceptor(Func<string, DbParameterCollection, DateTime, DateTime, object> executedInterceptor) {
            ExecutedInterceptorAction += executedInterceptor ?? throw new ArgumentNullException(nameof(executedInterceptor));
            return this;
        }

        /// <summary>
        /// Add long time executed interceptor
        /// </summary>
        /// <param name="executedInterceptor"></param>
        /// <param name="asAppend"></param>
        /// <returns></returns>
        public EfEnricherOptions AddLongTimeExecutedInterceptor(Func<string, DbParameterCollection, DateTime, DateTime, object> executedInterceptor, bool asAppend = false) {
            LongTimeExecutedInterceptorAction += executedInterceptor ?? throw new ArgumentNullException(nameof(executedInterceptor));
            if (asAppend) {
                AddExecutedInterceptor(executedInterceptor);
            }

            return this;
        }

        /// <summary>
        /// Add error interceptor
        /// </summary>
        /// <param name="errorInterceptor"></param>
        /// <returns></returns>
        public EfEnricherOptions AddErrorInterceptor(Func<Exception, string, DbParameterCollection, DateTime, DateTime, object> errorInterceptor) {
            ErrorInterceptorAction += errorInterceptor ?? throw new ArgumentNullException(nameof(errorInterceptor));
            return this;
        }


        internal Func<Exception, string, DbParameterCollection, DateTime, DateTime, object> NonQueryErrorInterceptorAction { get; set; }
        internal Action<string, DbParameterCollection, DateTime> NonQueryExecutingInterceptorAction { get; set; }
        internal Func<string, DbParameterCollection, DateTime, DateTime, object> NonQueryExecutedInterceptorAction { get; set; }
        internal Func<string, DbParameterCollection, DateTime, DateTime, object> NonQueryLongTimeExecutedInterceptorAction { get; set; }

        /// <summary>
        /// Add non query executing interceptor
        /// </summary>
        /// <param name="executingInterceptor"></param>
        /// <returns></returns>
        public EfEnricherOptions AddNonQueryExecutingInterceptor(Action<string, DbParameterCollection, DateTime> executingInterceptor) {
            NonQueryExecutingInterceptorAction += executingInterceptor ?? throw new ArgumentNullException(nameof(executingInterceptor));
            return this;
        }

        /// <summary>
        /// Add non query executed interceptor
        /// </summary>
        /// <param name="executedInterceptor"></param>
        /// <returns></returns>
        public EfEnricherOptions AddNonQueryExecutedInterceptor(Func<string, DbParameterCollection, DateTime, DateTime, object> executedInterceptor) {
            NonQueryExecutedInterceptorAction += executedInterceptor ?? throw new ArgumentNullException(nameof(executedInterceptor));
            return this;
        }

        /// <summary>
        /// Add non query long time executed interceptor
        /// </summary>
        /// <param name="executedInterceptor"></param>
        /// <param name="asAppend"></param>
        /// <returns></returns>
        public EfEnricherOptions AddNonQueryLongTimeExecutedInterceptor(Func<string, DbParameterCollection, DateTime, DateTime, object> executedInterceptor,
            bool asAppend = false) {
            NonQueryLongTimeExecutedInterceptorAction += executedInterceptor ?? throw new ArgumentNullException(nameof(executedInterceptor));
            if (asAppend) {
                AddNonQueryExecutedInterceptor(executedInterceptor);
            }

            return this;
        }

        /// <summary>
        /// Add non query error interceptor
        /// </summary>
        /// <param name="errorInterceptor"></param>
        /// <returns></returns>
        public EfEnricherOptions AddNonQueryErrorInterceptor(Func<Exception, string, DbParameterCollection, DateTime, DateTime, object> errorInterceptor) {
            NonQueryErrorInterceptorAction += errorInterceptor ?? throw new ArgumentNullException(nameof(errorInterceptor));
            return this;
        }


        internal Func<Exception, string, DbParameterCollection, DateTime, DateTime, object> ReaderErrorInterceptorAction { get; set; }
        internal Action<string, DbParameterCollection, DateTime> ReaderExecutingInterceptorAction { get; set; }
        internal Func<string, DbParameterCollection, DateTime, DateTime, object> ReaderExecutedInterceptorAction { get; set; }
        internal Func<string, DbParameterCollection, DateTime, DateTime, object> ReaderLongTimeExecutedInterceptorAction { get; set; }

        /// <summary>
        /// Add reader executing interceptor
        /// </summary>
        /// <param name="executingInterceptor"></param>
        /// <returns></returns>
        public EfEnricherOptions AddReaderExecutingInterceptor(Action<string, DbParameterCollection, DateTime> executingInterceptor) {
            ReaderExecutingInterceptorAction += executingInterceptor ?? throw new ArgumentNullException(nameof(executingInterceptor));
            return this;
        }

        /// <summary>
        /// Add reader executed interceptor
        /// </summary>
        /// <param name="executedInterceptor"></param>
        /// <returns></returns>
        public EfEnricherOptions AddReaderExecutedInterceptor(Func<string, DbParameterCollection, DateTime, DateTime, object> executedInterceptor) {
            ReaderExecutedInterceptorAction += executedInterceptor ?? throw new ArgumentNullException(nameof(executedInterceptor));
            return this;
        }

        /// <summary>
        /// Add reader long time executed interceptor
        /// </summary>
        /// <param name="executedInterceptor"></param>
        /// <param name="asAppend"></param>
        /// <returns></returns>
        public EfEnricherOptions AddReaderLongTimeExecutedInterceptor(Func<string, DbParameterCollection, DateTime, DateTime, object> executedInterceptor, bool asAppend = false) {
            ReaderLongTimeExecutedInterceptorAction += executedInterceptor ?? throw new ArgumentNullException(nameof(executedInterceptor));
            if (asAppend) {
                AddReaderExecutedInterceptor(executedInterceptor);
            }

            return this;
        }

        /// <summary>
        /// Add reader error interceptor
        /// </summary>
        /// <param name="errorInterceptor"></param>
        /// <returns></returns>
        public EfEnricherOptions AddReaderErrorInterceptor(Func<Exception, string, DbParameterCollection, DateTime, DateTime, object> errorInterceptor) {
            ReaderErrorInterceptorAction += errorInterceptor ?? throw new ArgumentNullException(nameof(errorInterceptor));
            return this;
        }


        internal Func<Exception, string, DbParameterCollection, DateTime, DateTime, object> ScalarErrorInterceptorAction { get; set; }
        internal Action<string, DbParameterCollection, DateTime> ScalarExecutingInterceptorAction { get; set; }
        internal Func<string, DbParameterCollection, DateTime, DateTime, object> ScalarExecutedInterceptorAction { get; set; }
        internal Func<string, DbParameterCollection, DateTime, DateTime, object> ScalarLongTimeExecutedInterceptorAction { get; set; }

        /// <summary>
        /// Add scalar executing interceptor
        /// </summary>
        /// <param name="executingInterceptor"></param>
        /// <returns></returns>
        public EfEnricherOptions AddScalarExecutingInterceptor(Action<string, DbParameterCollection, DateTime> executingInterceptor) {
            ScalarExecutingInterceptorAction += executingInterceptor ?? throw new ArgumentNullException(nameof(executingInterceptor));
            return this;
        }

        /// <summary>
        /// Add scalar executed interceptor
        /// </summary>
        /// <param name="executedInterceptor"></param>
        /// <returns></returns>
        public EfEnricherOptions AddScalarExecutedInterceptor(Func<string, DbParameterCollection, DateTime, DateTime, object> executedInterceptor) {
            ScalarExecutedInterceptorAction += executedInterceptor ?? throw new ArgumentNullException(nameof(executedInterceptor));
            return this;
        }

        /// <summary>
        /// Add scalar long time executed interceptor
        /// </summary>
        /// <param name="executedInterceptor"></param>
        /// <param name="asAppend"></param>
        /// <returns></returns>
        public EfEnricherOptions AddScalarLongTimeExecutedInterceptor(Func<string, DbParameterCollection, DateTime, DateTime, object> executedInterceptor, bool asAppend = false) {
            ScalarLongTimeExecutedInterceptorAction += executedInterceptor ?? throw new ArgumentNullException(nameof(executedInterceptor));
            if (asAppend) {
                AddScalarExecutedInterceptor(executedInterceptor);
            }

            return this;
        }

        /// <summary>
        /// Add scalar error interceptor
        /// </summary>
        /// <param name="errorInterceptor"></param>
        /// <returns></returns>
        public EfEnricherOptions AddScalarErrorInterceptor(Func<Exception, string, DbParameterCollection, DateTime, DateTime, object> errorInterceptor) {
            ScalarErrorInterceptorAction += errorInterceptor ?? throw new ArgumentNullException(nameof(errorInterceptor));
            return this;
        }

        #endregion

        #region Appeng filter

        internal Func<string, LogEventLevel, bool> Filter { get; set; }

        /// <summary>
        /// Use filter
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public EfEnricherOptions UseFilter(Func<string, LogEventLevel, bool> filter) {
            if (filter == null) throw new ArgumentNullException(nameof(filter));

            var temp = Filter;
            Filter = (s, l) => (temp?.Invoke(s, l) ?? true) && filter(s, l);

            return this;
        }

        #endregion

        #region Append output

        private readonly RenderingConfiguration _renderingOptions = new RenderingConfiguration();

        /// <inheritdoc />
        public EfEnricherOptions EnableDisplayCallerInfo(bool? displayingCallerInfoEnabled) {
            _renderingOptions.DisplayingCallerInfoEnabled = displayingCallerInfoEnabled;
            return this;
        }

        /// <inheritdoc />
        public EfEnricherOptions EnableDisplayEventIdInfo(bool? displayingEventIdInfoEnabled) {
            _renderingOptions.DisplayingEventIdInfoEnabled = displayingEventIdInfoEnabled;
            return this;
        }

        /// <inheritdoc />
        public EfEnricherOptions EnableDisplayNewLineEom(bool? displayingNewLineEomEnabled) {
            _renderingOptions.DisplayingNewLineEomEnabled = displayingNewLineEomEnabled;
            return this;
        }

        /// <inheritdoc />
        public RenderingConfiguration GetRenderingOptions() => _renderingOptions;

        #endregion

    }
}