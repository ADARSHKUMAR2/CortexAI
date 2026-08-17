using UnityEngine;

namespace CortexAI
{
    public enum EnvironmentType
    {
        Local,
        Production
    }

    [CreateAssetMenu(fileName = "CortexAIConfig", menuName = "CortexAI/API Configuration")]
    public class CortexAIConfig : ScriptableObject
    {
        [Header("Environment Settings")]
        public EnvironmentType CurrentEnvironment = EnvironmentType.Local;

        [Header("API Endpoints")]
        public string LocalBaseUrl = "http://192.168.1.9:8000";
        public string ProductionBaseUrl = "https://gateway-service-165920197771.asia-south1.run.app";

        public string GetCurrentUrl()
        {
            return CurrentEnvironment == EnvironmentType.Production ? ProductionBaseUrl : LocalBaseUrl;
        }
    }
}
