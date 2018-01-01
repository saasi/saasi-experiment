using System.Collections.Generic;
using Nexogen.Libraries.Metrics;
using Nexogen.Libraries.Metrics.Prometheus;

namespace Saasi.Monolithic.BusinessWeb {
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
                .LabelNames("io", "cpu", "memory", "timetorun")
                .Register();
            this.AddGauge("bms_active_transactions", bms);

            var vio = metrics.Counter()
                .Name("bms_business_violation_total")
                .Help("The number of business violations")
                .LabelNames("io", "cpu", "memory", "timetorun")
                .Register();
            this.AddCounter("bms_business_violation_total", vio);
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