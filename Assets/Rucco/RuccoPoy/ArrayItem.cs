namespace RuccoPoyLang
{
    class ArrayItem
    {
        public string key, value;

        public ArrayItem(string key, string value)
        {
            this.key = key;
            this.value = value;
        }
        public override string ToString()
        {
            string key = this.key == null ? "" : $"{{{this.key}}}:";
            return $"[{key}{{{value}}}]";
        }
    }
}
