﻿using EntityData;
using NLog;
using QDMS;
using QDMSServer;
using System;

namespace QDMSService
{
    public sealed class DataServer
    {
        private Logger _log;
        private Config.DataServiceConfig _config;

        private InstrumentManager _instrumentManager;

        private HistoricalDataBroker _historicalDataBroker;
        private RealTimeDataBroker _realTimeDataBroker;

        private InstrumentsServer _instrumentsServer;
        private HistoricalDataServer _historicalDataServer;
        private RealTimeDataServer _realTimeDataServer;
        

        public DataServer(Config.DataServiceConfig config)
        {
            _config = config;
            _log = LogManager.GetCurrentClassLogger();
        }

        public void Initialisize()
        {
            _log.Info($"Server is initialisizing ...");

            //create data db if it doesn't exist
            /*DataDBContext dataContext;
            try
            {
                dataContext = new DataDBContext();
                //dataContext.Database.Initialize(false);
            }
            catch (System.Data.Entity.Core.ProviderIncompatibleException ex)
            {
                throw new NotSupportedException("Could not connect to context DataDB!", ex);
            }
            dataContext.Dispose();

            MyDBContext entityContext;
            try
            {
                entityContext = new MyDBContext();
                //entityContext.Database.Initialize(false);
            }
            catch (System.Data.Entity.Core.ProviderIncompatibleException ex)
            {
                throw new NotSupportedException("Could not connect to context MyDB!", ex);
            }*/

            // initialisize helper classes
            _instrumentManager = new InstrumentManager(_config.LocalStorage.BuildDbContextOptions<MyDBContext>());

            var cfRealtimeBroker = new ContinuousFuturesBroker(new QDMSClient.QDMSClient("RTDBCFClient", "127.0.0.1",
                _config.RealtimeDataService.RequestPort, _config.RealtimeDataService.PublisherPort,
                _config.InstrumentService.Port, _config.HistoricalDataService.Port), _instrumentManager, false);
            var cfHistoricalBroker = new ContinuousFuturesBroker(new QDMSClient.QDMSClient("HDBCFClient", "127.0.0.1",
                _config.RealtimeDataService.RequestPort, _config.RealtimeDataService.PublisherPort,
                _config.InstrumentService.Port, _config.HistoricalDataService.Port), _instrumentManager, false);

            IDataStorage localStorage;

            switch (_config.LocalStorage.Type)
            {
                case Config.DatabaseConnectionType.MySql:
                    throw new NotImplementedException();
                    // @Todo
                    //localStorage = new QDMSServer.DataSources.MySQLStorage(_config.LocalStorage.ConnectionString);
                    //break;
                case Config.DatabaseConnectionType.SqlServer:
                    localStorage = new QDMSServer.DataSources.SqlServerStorage(_config.LocalStorage.ConnectionString);
                    break;
                case Config.DatabaseConnectionType.Sqlite:
                    localStorage = new QDMS.Server.DataStorage.Sqlite.SqliteStorage(_config.LocalStorage.ConnectionString);
                    break;
                default:
                    throw new NotSupportedException("Not supported local storage type: " + _config.LocalStorage.Type);
            }

            // create brokers
            _historicalDataBroker = new HistoricalDataBroker(cfHistoricalBroker, localStorage, new IHistoricalDataSource[] { });
            _realTimeDataBroker = new RealTimeDataBroker(cfRealtimeBroker, localStorage, new IRealTimeDataSource[] { });

            // create servers
            _instrumentsServer = new InstrumentsServer(_config.InstrumentService.Port, _instrumentManager);
            _historicalDataServer = new HistoricalDataServer(_config.HistoricalDataService.Port, _historicalDataBroker);
            _realTimeDataServer = new RealTimeDataServer(_config.RealtimeDataService.PublisherPort, _config.RealtimeDataService.RequestPort, _realTimeDataBroker);

            // ... start the servers
            _instrumentsServer.StartServer();
            _historicalDataServer.StartServer();
            _realTimeDataServer.StartServer();

            _log.Info($"Server is ready. Fall into sleep mode.");

            while (true)
            { System.Threading.Thread.Sleep(60000); }
        }

        public void Stop()
        {
            _realTimeDataServer.Dispose();
            _historicalDataBroker.Dispose();
            _instrumentsServer.Dispose();
        }
    }   
}
