using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace WebApplication3.Models
{
    public class Question
    {
        [Required]
        public int Id { get; set; }
        [Required(ErrorMessage = "이름을 입력하세요.")]
        public string Name { get; set; }
        [Required(ErrorMessage = "이메일을 입력하세요.")]
        [EmailAddress(ErrorMessage = "유효한 이메일 주소를 입력하세요.")]
        public string Email { get; set; }
        [Required(ErrorMessage = "제목을 입력하세요.")]
        public string Title { get; set; }
        [Required(ErrorMessage = "내용을 입력하세요.")]
        public string Content { get; set; }
        [Required]
        public string Date { get; set; }
    }

    public class Answer
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public string Email { get; set; }
        [Required(ErrorMessage = "내용을 입력하세요.")]
        public string Content { get; set; }
        [Required]
        public string Date { get; set; }
        [Required]
        public string MailDate { get; set; }
    }

    public class QuestionAnswer
    {
        public Question Question { get; set; }
        public Answer Answer { get; set; }
    }

    public class QnaViewModel : DbContext
    {
        public QnaViewModel(DbContextOptions<QnaViewModel> options) : base(options)
        {
        }

        public DbSet<Question> Questions { get; set; }
        public DbSet<Answer> Answers { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 클래스에 대한 테이블 이름 설정
            modelBuilder.Entity<Question>().ToTable("question");
            modelBuilder.Entity<Answer>().ToTable("answer");
        }
    }
}