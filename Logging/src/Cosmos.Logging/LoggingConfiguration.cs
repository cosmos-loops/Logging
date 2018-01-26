﻿using System;
using System.Collections.Generic;
using Cosmos.Logging.Configurations;
using Cosmos.Logging.Events;
using Cosmos.Logging.Filters;
using EnumsNET;
using Microsoft.Extensions.Configuration;

namespace Cosmos.Logging {
    public class LoggingConfiguration {
        private readonly IConfigurationRoot _loggingConfiguration;
        private readonly NamespaceFilterNavCache _namespaceFilterNavCache;
        private readonly List<NamespaceFilterNav> _namespaceFilterNavRoots = new List<NamespaceFilterNav>();
        private readonly Dictionary<string, SinkConfiguration> _sinkConfigurations = new Dictionary<string, SinkConfiguration>();
        private readonly object _parsedLgLevelLock = new object();
        private readonly object _sinkConfigurationsLock = new object();

        public LoggingConfiguration() { }

        public LoggingConfiguration(IConfigurationRoot loggingConfiguration) {
            _loggingConfiguration = loggingConfiguration ?? throw new ArgumentNullException(nameof(loggingConfiguration));
            _namespaceFilterNavCache = new NamespaceFilterNavCache(new NamespaceFilterNavParser());
            SetSelf(loggingConfiguration.GetSection("Logging").Get<LoggingConfiguration>());
        }

        public bool IncludeScopes { get; set; }

        public Dictionary<string, string> LogLevel { get; set; } = new Dictionary<string, string>();

        public IReadOnlyList<NamespaceFilterNav> NamespaceFilterNavs => _namespaceFilterNavRoots;

        public LogEventLevel GetDefaultMinimumLevel() {
            return LogLevel.TryGetValue("Default", out var strLevel)
                ? Enums.GetMember<LogEventLevel>(strLevel, true).Value
                : LogEventLevel.Verbose;
        }

        public LogEventLevel GetSinkDefaultMinimumLevel(string sinkName) {
            return GetSinkConfiguration(sinkName)?.GetDefaultMinimumLevel() ?? LogEventLevel.Off;
        }

        public DestructureConfiguration Destructure { get; set; }

        public IConfigurationRoot Configuration => _loggingConfiguration;

        public void SetSinkConfiguration<TSinkConfiguration>(string sectionName, Action<IConfiguration, TSinkConfiguration> addtionalAct = null)
            where TSinkConfiguration : SinkConfiguration, new() {
            if (string.IsNullOrWhiteSpace(sectionName)) throw new ArgumentNullException(nameof(sectionName));
            var section = _loggingConfiguration.GetSection(sectionName);
            var configuration = section.Get<TSinkConfiguration>() ?? new TSinkConfiguration();
            addtionalAct?.Invoke(section, configuration);

            SetSinkConfiguration(configuration);
        }

        public void SetSinkConfiguration<TSinkConfiguration>(TSinkConfiguration configuration)
            where TSinkConfiguration : SinkConfiguration, new() {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            if (string.IsNullOrWhiteSpace(configuration.Name)) throw new ArgumentNullException(nameof(configuration.Name));
            // ReSharper disable once InconsistentlySynchronizedField
            if (!_sinkConfigurations.ContainsKey(configuration.Name)) {
                lock (_sinkConfigurationsLock) {
                    if (!_sinkConfigurations.ContainsKey(configuration.Name)) {
                        configuration.UpdateParsedLogLevel(_namespaceFilterNavCache);
                        _sinkConfigurations.Add(configuration.Name, configuration);
                    }
                }
            }
        }

        public TSinkConfiguration GetSinkConfiguration<TSinkConfiguration>() where TSinkConfiguration : SinkConfiguration, new() {
            var temp = new TSinkConfiguration();
            return GetSinkConfiguration<TSinkConfiguration>(temp.Name);
        }

        public TSinkConfiguration GetSinkConfiguration<TSinkConfiguration>(string name) where TSinkConfiguration : SinkConfiguration, new() {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));
            return _sinkConfigurations[name] as TSinkConfiguration;
        }

        public SinkConfiguration GetSinkConfiguration(string name) {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));
            return _sinkConfigurations[name];
        }

        private void SetSelf(LoggingConfiguration configuration) {
            if (configuration == null) {
                IncludeScopes = false;
                LogLevel = new Dictionary<string, string> {{"Default", "Information"}};
            } else {
                IncludeScopes = configuration.IncludeScopes;
                LogLevel = configuration.LogLevel;
            }

            foreach (var item in LogLevel) {
                var nav = _namespaceFilterNavCache.Parse(item.Key, item.Value);
                if (nav is EmptyNamespaceFilterNav) continue;
                if (!_namespaceFilterNavRoots.Contains(nav)) {
                    lock (_parsedLgLevelLock) {
                        if (!_namespaceFilterNavRoots.Contains(nav)) {
                            _namespaceFilterNavRoots.Add(nav);
                        }
                    }
                }
            }
        }
    }
}