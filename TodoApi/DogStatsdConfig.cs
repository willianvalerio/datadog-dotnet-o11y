using StatsdClient;

namespace TodoApi

{
    public class DogStatsdConfig{
        readonly StatsdConfig statsdConfig;

        public DogStatsdConfig(){

            statsdConfig = new StatsdConfig{
                Prefix = "todoapi.dogstatsd",
                StatsdServerName = "unix:///var/run/datadog/dsd.socket"
            };            
        }

        public void IncrementMetric(string metricName, int sampleRate = 1, string[] tags = null){
            using(var dogStatsdService = new DogStatsdService()){
                dogStatsdService.Configure(statsdConfig);
                dogStatsdService.Increment(metricName,sampleRate: sampleRate,tags: tags);
            }
        }

        public void DecrementMetric(string metricName, int sampleRate = 1, string[] tags = null){
            using(var dogStatsdService = new DogStatsdService()){
                dogStatsdService.Configure(statsdConfig);
                dogStatsdService.Decrement(metricName, sampleRate: sampleRate,tags: tags);
            }
        }

    }


}