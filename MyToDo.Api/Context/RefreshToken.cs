namespace MyToDo.Api.Context
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Token { get; set; } = string.Empty;   // 随机字符串
        public DateTime ExpiresAt { get; set; }              // 过期时间
        public DateTime CreatedAt { get; set; }
        public DateTime? RevokedAt { get; set; }             // 吊销时间，null 表示有效

        public bool IsActive => RevokedAt == null && DateTime.UtcNow < ExpiresAt;

        public User? User { get; set; }
    }
}