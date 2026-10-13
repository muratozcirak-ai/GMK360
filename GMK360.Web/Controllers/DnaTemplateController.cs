using GMK360.Core.Entities.Construction.Workflows;
using GMK360.Data.Contexts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace GMK360.Web.Controllers
{
    public class DnaTemplateController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DnaTemplateController(ApplicationDbContext context)
        {
            _context = context;
        }

        // --- EXİSTİNG METHODS (I will preserve them) ---

        public IActionResult Index()
        {
            var steps = _context.DnaTemplateSteps.OrderBy(s => s.StepOrder).ToList();
            return View(steps);
        }

        [HttpPost]
        public IActionResult AddStep(string stepName, int stepOrder)
        {
            if (!string.IsNullOrWhiteSpace(stepName))
            {
                var step = new DnaTemplateStep
                {
                    StepName = stepName.Trim(),
                    StepOrder = stepOrder
                };
                _context.DnaTemplateSteps.Add(step);
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult UpdateStep(int id, string stepName, int stepOrder)
        {
            var step = _context.DnaTemplateSteps.Find(id);
            if (step != null && !string.IsNullOrWhiteSpace(stepName))
            {
                step.StepName = stepName.Trim();
                step.StepOrder = stepOrder;
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult DeleteStep(int id)
        {
            var step = _context.DnaTemplateSteps.Include(s => s.Questions).FirstOrDefault(s => s.Id == id);
            if (step != null)
            {
                _context.DnaTemplateSteps.Remove(step);
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        public IActionResult Questions(int stepId)
        {
            var step = _context.DnaTemplateSteps.Find(stepId);
            if (step == null) return NotFound();

            var questions = _context.DnaTemplateQuestions
                .Include(q => q.Profession)
                .Include(q => q.Options)
                .Where(q => q.DnaTemplateStepId == stepId)
                .OrderBy(q => q.QuestionOrder)
                .ToList();

            ViewBag.Step = step;
            ViewBag.StepId = step.Id;
            ViewBag.StepName = step.StepName;
            ViewBag.Professions = _context.DnaProfessions.OrderBy(p => p.Name).ToList();

            return View(questions);
        }

        [HttpPost]
        public IActionResult AddQuestion(int stepId, string title, int? dnaProfessionId, string notes, string optionSource, bool isMultipleChoice, int questionOrder)
        {
            if (!string.IsNullOrWhiteSpace(title))
            {
                var question = new DnaTemplateQuestion
                {
                    DnaTemplateStepId = stepId,
                    Title = title,
                    DnaProfessionId = dnaProfessionId,
                    OptionSource = optionSource ?? "",
                    Notes = notes ?? "",
                    IsMultipleChoice = isMultipleChoice,
                    IsRequired = true,
                    QuestionOrder = questionOrder
                };
                _context.DnaTemplateQuestions.Add(question);
                _context.SaveChanges();
            }
            return RedirectToAction("Questions", new { stepId = stepId });
        }

        [HttpPost]
        public IActionResult UpdateQuestion(int id, string title, int? dnaProfessionId, string notes, string optionSource, bool isMultipleChoice, int questionOrder)
        {
            var question = _context.DnaTemplateQuestions.Find(id);
            if (question != null && !string.IsNullOrWhiteSpace(title))
            {
                question.Title = title;
                question.DnaProfessionId = dnaProfessionId;
                question.OptionSource = optionSource ?? "";
                question.Notes = notes ?? "";
                question.IsMultipleChoice = isMultipleChoice;
                question.QuestionOrder = questionOrder;
                _context.SaveChanges();
            }
            return RedirectToAction("Questions", new { stepId = question.DnaTemplateStepId });
        }

        [HttpPost]
        public IActionResult DeleteQuestion(int id)
        {
            var question = _context.DnaTemplateQuestions.Find(id);
            if (question != null)
            {
                int stepId = question.DnaTemplateStepId;
                _context.DnaTemplateQuestions.Remove(question);
                _context.SaveChanges();
                return RedirectToAction("Questions", new { stepId = stepId });
            }
            return RedirectToAction("Index");
        }

        // --- PROFESSIONS ---
        public IActionResult Professions()
        {
            var professions = _context.DnaProfessions.OrderBy(p => p.Name).ToList();
            return View(professions);
        }

        [HttpPost]
        public IActionResult AddProfession(string name)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                _context.DnaProfessions.Add(new DnaProfession { Name = name });
                _context.SaveChanges();
            }
            return RedirectToAction("Professions");
        }

        [HttpPost]
        public IActionResult UpdateProfession(int id, string name)
        {
            var prof = _context.DnaProfessions.Find(id);
            if (prof != null && !string.IsNullOrWhiteSpace(name))
            {
                prof.Name = name;
                _context.SaveChanges();
            }
            return RedirectToAction("Professions");
        }

        [HttpPost]
        public IActionResult DeleteProfession(int id)
        {
            var prof = _context.DnaProfessions.Find(id);
            if (prof != null)
            {
                _context.DnaProfessions.Remove(prof);
                _context.SaveChanges();
            }
            return RedirectToAction("Professions");
        }

        // --- OPTIONS (SEÇENEKLER) ---
        public IActionResult Options(int questionId)
        {
            var question = _context.DnaTemplateQuestions.Find(questionId);
            if (question == null) return NotFound();

            var options = _context.DnaTemplateOptions
                .Include(o => o.Profession)
                .Where(o => o.DnaTemplateQuestionId == questionId)
                .OrderBy(o => o.OptionOrder)
                .ToList();

            ViewBag.QuestionId = questionId;
            ViewBag.StepId = question.DnaTemplateStepId;
            ViewBag.QuestionTitle = question.Title;
            ViewBag.OptionSource = question.OptionSource;
            ViewBag.Professions = _context.DnaProfessions.OrderBy(p => p.Name).ToList();

            return View(options);
        }

        [HttpPost]
        public IActionResult AddOption(int questionId, string optionText, int? dnaProfessionId, int optionOrder)
        {
            if (!string.IsNullOrWhiteSpace(optionText))
            {
                var option = new DnaTemplateOption
                {
                    DnaTemplateQuestionId = questionId,
                    OptionText = optionText,
                    DnaProfessionId = dnaProfessionId,
                    OptionOrder = optionOrder
                };
                _context.DnaTemplateOptions.Add(option);
                _context.SaveChanges();
            }
            return RedirectToAction("Options", new { questionId = questionId });
        }

        [HttpPost]
        public IActionResult UpdateOption(int id, int questionId, string optionText, int? dnaProfessionId, int optionOrder)
        {
            var option = _context.DnaTemplateOptions.Find(id);
            if (option != null && !string.IsNullOrWhiteSpace(optionText))
            {
                option.OptionText = optionText;
                option.DnaProfessionId = dnaProfessionId;
                option.OptionOrder = optionOrder;
                _context.SaveChanges();
            }
            return RedirectToAction("Options", new { questionId = questionId });
        }

        [HttpPost]
        public IActionResult DeleteOption(int id, int questionId)
        {
            var option = _context.DnaTemplateOptions.Find(id);
            if (option != null)
            {
                _context.DnaTemplateOptions.Remove(option);
                _context.SaveChanges();
            }
            return RedirectToAction("Options", new { questionId = questionId });
        }
    }
}