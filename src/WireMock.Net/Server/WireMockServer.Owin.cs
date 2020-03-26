using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using JetBrains.Annotations;
using Newtonsoft.Json;
#if !USE_ASPNETCORE
using Owin;
#endif
using WireMock.Admin.Mappings;
using WireMock.Exceptions;
using WireMock.Handlers;
using WireMock.Logging;
using WireMock.Owin;
using WireMock.Serialization;
using WireMock.Settings;
using WireMock.Validation;

namespace WireMock.Server
{
  public partial class WireMockServer
  {
#if !USE_ASPNETCORE
    [PublicAPI]
    public static WireMockServer StartForOwin(IAppBuilder p_app, IWireMockServerSettings p_settings)
    {
      return new WireMockServer(p_app, p_settings);
    }

    protected WireMockServer(IAppBuilder p_app, IWireMockServerSettings settings)
    {
      _settings = settings;

      // Set default values if not provided
      _settings.Logger = settings.Logger ?? new WireMockNullLogger();
      _settings.FileSystemHandler = settings.FileSystemHandler ?? new LocalFileSystemHandler();

      _settings.Logger.Info("WireMock.Net by Stef Heyenrath (https://github.com/WireMock-Net/WireMock.Net)");
      _settings.Logger.Debug("WireMock.Net server settings {0}", JsonConvert.SerializeObject(settings, Formatting.Indented));

      HostUrlOptions urlOptions;
      if (settings.Urls != null)
      {
        urlOptions = new HostUrlOptions
        {
          Urls = settings.Urls
        };
      }
      else
      {
        urlOptions = new HostUrlOptions
        {
          UseSSL = settings.UseSSL == true
        };
      }

      _options.FileSystemHandler = _settings.FileSystemHandler;
      _options.PreWireMockMiddlewareInit = _settings.PreWireMockMiddlewareInit;
      _options.PostWireMockMiddlewareInit = _settings.PostWireMockMiddlewareInit;
      _options.Logger = _settings.Logger;
      _options.DisableJsonBodyParsing = _settings.DisableJsonBodyParsing;
      _options.IgnorePrefixURLs = _settings.IgnorePrefixURLs;

      _matcherMapper = new MatcherMapper(_settings);
      _mappingConverter = new MappingConverter(_matcherMapper);

      _httpServer = new OwinIISHost(_options, urlOptions, p_app);

      var startTask = _httpServer.StartAsync();

      using (var ctsStartTimeout = new CancellationTokenSource(settings.StartTimeout))
      {
        while (!_httpServer.IsStarted)
        {
          // Throw exception if service start fails
          if (_httpServer.RunningException != null)
          {
            throw new WireMockException($"Service start failed with error: {_httpServer.RunningException.Message}", _httpServer.RunningException);
          }

          if (ctsStartTimeout.IsCancellationRequested)
          {
            // In case of an aggregate exception, throw the exception.
            if (startTask.Exception != null)
            {
              throw new WireMockException($"Service start failed with error: {startTask.Exception.Message}", startTask.Exception);
            }

            // Else throw TimeoutException
            throw new TimeoutException($"Service start timed out after {TimeSpan.FromMilliseconds(settings.StartTimeout)}");
          }

          ctsStartTimeout.Token.WaitHandle.WaitOne(ServerStartDelayInMs);
        }

        Urls = _httpServer.Urls.ToArray();
        Ports = _httpServer.Ports;
      }

      if (settings.AllowBodyForAllHttpMethods == true)
      {
        _options.AllowBodyForAllHttpMethods = _settings.AllowBodyForAllHttpMethods;
        _settings.Logger.Info("AllowBodyForAllHttpMethods is set to True");
      }

      if (settings.AllowOnlyDefinedHttpStatusCodeInResponse == true)
      {
        _options.AllowOnlyDefinedHttpStatusCodeInResponse = _settings.AllowOnlyDefinedHttpStatusCodeInResponse;
        _settings.Logger.Info("AllowOnlyDefinedHttpStatusCodeInResponse is set to True");
      }

      if (settings.AllowPartialMapping == true)
      {
        AllowPartialMapping();
      }

      if (settings.StartAdminInterface == true)
      {
        if (!string.IsNullOrEmpty(settings.AdminUsername) && !string.IsNullOrEmpty(settings.AdminPassword))
        {
          SetBasicAuthentication(settings.AdminUsername, settings.AdminPassword);
        }

        InitAdmin();
      }

      if (settings.ReadStaticMappings == true)
      {
        ReadStaticMappings();
      }

      if (settings.WatchStaticMappings == true)
      {
        WatchStaticMappings();
      }

      if (settings.ProxyAndRecordSettings != null)
      {
        InitProxyAndRecord(settings);
      }

      if (settings.RequestLogExpirationDuration != null)
      {
        SetRequestLogExpirationDuration(settings.RequestLogExpirationDuration);
      }

      if (settings.MaxRequestLogCount != null)
      {
        SetMaxRequestLogCount(settings.MaxRequestLogCount);
      }
    }
#endif
  }
}