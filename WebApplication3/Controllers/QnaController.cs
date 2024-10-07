using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using WebApplication3.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace WebApplication3.Controllers
{
    //[Authorize(Policy = "RequireUserRole")]
    //[Authorize(Policy = "RequireAuthenticatedUser")]
    [Authorize(Roles = "Admin")]

    public class QnaController : Controller
    {
        private readonly QnaViewModel _question;
        private readonly QnaViewModel _answer;
        private readonly GmailEmailService _emailService;

        public QnaController(QnaViewModel question, QnaViewModel answer, GmailEmailService emailService)
        {
            _question = question;
            _answer = answer;
            _emailService = emailService;
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult QuestionWrite()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> QuestionWrite(Question question)
        {
            question.Date = DateTime.Now.ToString("yyyy-MM-dd tt hh:mm:ss");
            _question.Questions.Add(question);
            await _question.SaveChangesAsync();
            return RedirectToAction("qnalist");
        }

        public IActionResult Qnalist(string searchFilter, string searchQuery)
        {
            var questions = _question.Questions.AsQueryable();
            var answers = _answer.Answers.AsQueryable();

            if (!string.IsNullOrEmpty(searchQuery))
            {
                switch (searchFilter)
                {
                    case "number":
                        questions = questions.Where(q => q.Id.ToString().Contains(searchQuery));
                        break;
                    case "title":
                        questions = questions.Where(q => q.Title.Contains(searchQuery));
                        break;
                    case "email":
                        questions = questions.Where(q => q.Email.Contains(searchQuery));
                        break;
                    case "all":
                        questions = questions.Where(q => q.Id.ToString().Contains(searchQuery) || q.Title.Contains(searchQuery) || q.Email.Contains(searchQuery));
                        break;
                }
            }

            var questionAnswersList = questions.GroupJoin(answers,
                question => question.Id,
                answer => answer.Id,
                (question, answerList) => new QuestionAnswer
                {
                    Question = question,
                    Answer = answerList.FirstOrDefault()
                }).ToList();

            return View(questionAnswersList);
        }


        public async Task<IActionResult> Detail(int id)
        {
            var question = await _question.Questions.FirstOrDefaultAsync(q => q.Id == id);
            var answer = await _answer.Answers.FirstOrDefaultAsync(a => a.Id == id);

            // 답변이 없을 경우에는 질문만 포함된 모델을 전달
            if (answer == null)
            {
                var qnaModel = new QuestionAnswer
                {
                    Question = question,
                    Answer = null
                };
                return PartialView(qnaModel);
            }
            else
            {
                var qnaModel = new QuestionAnswer
                {
                    Question = question,
                    Answer = answer
                };
                return PartialView(qnaModel);
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteQuestion(int id)
        {
            var question = await _question.Questions.FirstOrDefaultAsync(q => q.Id == id); // null?
            var answer = await _answer.Answers.FirstOrDefaultAsync(a => a.Id == id);

            if (question != null)
            {
                _question.Questions.Remove(question);
                _answer.Answers.Remove(answer);
            }
            await _question.SaveChangesAsync();
            await _answer.SaveChangesAsync();

            return RedirectToAction("qnalist");
        }

        [HttpGet]
        public async Task<IActionResult> Answer(int id)
        {
            var question = await _question.Questions.FirstOrDefaultAsync(q => q.Id == id);
            var answer = await _answer.Answers.FirstOrDefaultAsync(a => a.Id == id);
            if (question == null)
            {
                return NotFound();
            }
            var viewModel = new QuestionAnswer
            {
                Question = question,
                Answer = answer
            };

            return PartialView(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Answer(Answer answer, int id)
        {
            var existingAnswer = await _answer.Answers.FirstOrDefaultAsync(a => a.Id == id);
            if (existingAnswer != null)
            {
                // 기존 답변이 있는 경우 수정
                existingAnswer.Email = User.FindFirstValue(ClaimTypes.Email);
                existingAnswer.Content = answer.Content;
                answer = existingAnswer;
                _answer.Answers.Update(existingAnswer);
            }
            else
            {
                // 기존 답변이 없는 경우 새 답변을 추가
                answer.Id = id;
                answer.Email = User.FindFirstValue(ClaimTypes.Email);
                answer.Content = answer.Content;
                _answer.Answers.Add(answer);
            }
            // 공통
            answer.Date = DateTime.Now.ToString("yyyy-MM-dd tt hh:mm:ss");
            answer.MailDate = "";

            await _answer.SaveChangesAsync();
            return RedirectToAction("qnalist");
        }

        [HttpPost]
        public async Task<IActionResult> SendEmail(string recipientEmail, string subject, string message, int id)
        {
            var existingAnswer = await _answer.Answers.FirstOrDefaultAsync(a => a.Id == id);
            // 답변 내용이 없을 시
            if (existingAnswer == null)
            {
                TempData["message"] = "답변을 먼저 입력하세요.";
                return RedirectToAction("qnalist");
            }

            // 이메일 전송
            var result = await _emailService.SendEmailAsync(recipientEmail, subject, message);
            if (result)
            {
                existingAnswer.MailDate = DateTime.Now.ToString("yyyy-MM-dd tt hh:mm:ss");
                _answer.Answers.Update(existingAnswer);
                await _answer.SaveChangesAsync();
                TempData["message"] = "이메일 전송 성공";
            }
            else
            {
                TempData["message"] = "이메일 전송 실패";
            }
            return RedirectToAction("qnalist");
        }
    }
}
