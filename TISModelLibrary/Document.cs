namespace TISModelLibrary
{
    public class Document
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Extension { get; set; }
        public string Data { get; set; }

        public void LoadBytes(byte[] bytes)
        {
            Data = Document.SerializeBytes(bytes);
        }

        public byte[] GetBytes()
        {
            return Document.DeserializeBytes(Data);
        }

        public static string SerializeBytes(byte[] bytes)
        {
            return System.Convert.ToBase64String(bytes);
        }

        public static byte[] DeserializeBytes(string source)
        {
            return System.Convert.FromBase64String(source);
        }
    }
}