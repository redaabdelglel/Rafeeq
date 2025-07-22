
namespace Rafeeq.DTOs.Contact
{
    public class CreateContactDto
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
    }

    public class ContactMessageDto
    {
        public int MessageId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public string ResponderName { get; set; } 
    }

    /*public class ContactResponseDto
    {
        public string ResponseMessage { get; set; }
    }*/

    public class UpdateContactStatusDto
    {
        public string Status { get; set; }
    }
   
    public class ContactMessageListDto
    {
        public int MessageId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Subject { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
        public DateTime CreatedAt { get; set; }
    }
    
    public class CreateReplyDto
    {
        public int MessageId { get; set; }
        public string ReplyText { get; set; }
    }

    public class ContactReplyDto
    {
        public int ReplyId { get; set; }
        public string ReplyText { get; set; }
        public DateTime CreatedAt { get; set; }
        public string ResponderName { get; set; }  
    }
    public class ContactConversationDto
    {
        public string Email { get; set; }
        public List<ContactMessageDto> Messages { get; set; }
        public List<ContactReplyDto> Replies { get; set; }
    }
    
}
