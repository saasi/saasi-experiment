using System.Collections.Generic;
using Nexogen.Libraries.Metrics;
using Nexogen.Libraries.Metrics.Prometheus;

namespace Saasi.Microservices.Business {
    public interface IMetricsContainer {
        ILabelledGauge GetGauge(string name);
        ILabelledCounter GetCounter(string name);
    }

    public class MetricsContainer : IMetricsContainer {
        private readonly IMetrics _metrics;
        public MetricsContainer(IMetrics metrics)
        {
            this._metrics = metrics;
            var bms = metrics.Gauge()
                .Name("bms_active_transactions")
                .Help("The number of currently running requests.")
                .LabelNames("operation_id")
                .Register();
            this.AddGauge("bms_active_transactions", bms);

            var inq = metrics.Gauge()
                .Name("bms_in_queue")
                .Help("The number requests currently queuing.")
                .LabelNames("ok")
                .Register();
            this.AddGauge("bms_in_queue", inq);

            var exec = metrics.Gauge()
                .Name("bms_exec")
                .Help("The number currently executing requests.")
                .LabelNames("ok")
                .Register();
            this.AddGauge("bms_exec", exec);

            var vio = metrics.Counter()
                .Name("bms_business_violation_total")
                .Help("The number of business violations")
                .LabelNames("operation_id")
                .Register();
            this.AddCounter("bms_business_violation_total", vio);
            
            var total = metrics.Counter()
                .Name("bms_requests_served")
                .Help("The number of requests served")
                .LabelNames("operation_id")
                .Register();
            this.AddCounter("bms_requests_served", total);
        }
        
        private Dictionary<string, ILabelledGauge> gauges = new Dictionary<string, ILabelledGauge>();
        private Dictionary<string, ILabelledCounter> counters = new Dictionary<string, ILabelledCounter>();
        public ILabelledGauge GetGauge(string name) {
            return this.gauges[name];
        }

        public void AddGauge(string name, ILabelledGauge g) {
            this.gauges.Add(name, g);
        }

        public ILabelledCounter GetCounter(string name) {
           return this.counters[name]; 
        }

        public void AddCounter(string name, ILabelledCounter c) {
            this.counters.Add(name, c);
        }

    }
}