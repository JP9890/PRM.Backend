using System;

namespace PRMTool.Domain.Entities
{
    public class SystemSetting
    {
        public int Id { get; private set; }
        public string Key { get; private set; } = string.Empty;
        public string Value { get; private set; } = string.Empty;
        public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

        protected SystemSetting() { }

        public SystemSetting(string key, string value)
        {
            Key = key;
            Value = value;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateValue(string value)
        {
            Value = value;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
