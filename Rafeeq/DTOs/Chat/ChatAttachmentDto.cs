namespace Rafeeq.DTOs.Chat
{
    public class ChatAttachmentDto
    {
        public int AttachmentId { get; set; }
        public int MessageId { get; set; }
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public int FileSize { get; set; }
        public string ContentType { get; set; }
        public string FullUrl { get; set; }
        public bool IsVoiceMessage { get; set; }
    }
}
