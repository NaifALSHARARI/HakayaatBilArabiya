using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using HakayaatBilArabiya.Services;
using HakayaatBilArabiya.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace HakayaatBilArabiya.Controllers
{
    public class StoriesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly WatsonAssistantClient _watsonAssistantClient;
        private readonly AllamSynonymsService _synonymsService;

        public StoriesController(ApplicationDbContext context, WatsonAssistantClient watsonAssistantClient, AllamSynonymsService synonymsService)
        {
            _context = context;
            _watsonAssistantClient = watsonAssistantClient;
            _synonymsService = synonymsService;
        }

        public async Task<IActionResult> Index()
        {
            var stories = await _context.Stories.ToListAsync();
            return View(stories);
        }

        [HttpGet]
        public IActionResult GenerateStory()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateStory(string الشخصيات, string الحيوانات, string المكان, string الموسم, string الوقت, string inputText)
        {
            if (string.IsNullOrWhiteSpace(inputText) &&
                (string.IsNullOrWhiteSpace(الشخصيات) || string.IsNullOrWhiteSpace(الحيوانات) || string.IsNullOrWhiteSpace(المكان) ||
                     string.IsNullOrWhiteSpace(الوقت) || string.IsNullOrWhiteSpace(الموسم)))
            {
                ViewBag.Error = "يجب اختيار جميع العناصر (الشخصيات، الحيوانات، المكان، الوقت، والموسم) أو إدخال نص يدوي.";
                return View();
            }

            try
            {
                var بداية_القصة = "كان يا ما كان، ";

                var موضوع_القصة = string.IsNullOrWhiteSpace(inputText)
                    ? $"كان يا ما كان، {الشخصيات} كانوا يعيشون في {المكان} حيث كانوا يتفاعلون مع {الحيوانات}. خلال {الموسم} في {الوقت}، حدثت مغامرة لا تنسى!"
                    : $"{inputText}. هذه القصة تدور حول {الشخصيات} و {الحيوانات} في {المكان} خلال {الموسم} في {الوقت}.";

                var generatedStory = await _watsonAssistantClient.GenerateTextAsync(الشخصيات, الحيوانات, المكان, الموسم, الوقت, موضوع_القصة);

                var title = "قصة مولدة"; 

                if (!string.IsNullOrWhiteSpace(الشخصيات))
                {
                    title = $"قصة: {الشخصيات}";
                }

                if (!string.IsNullOrWhiteSpace(الحيوانات))
                {
                    title += $" و {الحيوانات}";
                }
                if (!string.IsNullOrWhiteSpace(الوقت))
                {
                    title += $" و {الوقت}";
                }
                if (!string.IsNullOrWhiteSpace(الموسم))
                {
                    title += $" و {الموسم}";
                }
                if (!string.IsNullOrWhiteSpace(المكان))
                {
                    title += $" و {المكان}";
                }

                if (!string.IsNullOrWhiteSpace(inputText))
                {
                    title = $"قصة مخصصة: {inputText.Substring(0, Math.Min(20, inputText.Length))}...";
                }

                var story = new Story
                {
                    Title = title,
                    Content = generatedStory
                };

                _context.Stories.Add(story);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"حدث خطأ أثناء توليد القصة: {ex.Message}";
                return View();
            }
        }

        [HttpPost]
        public async Task<IActionResult> GetSynonyms(string word)
        {
            if (string.IsNullOrWhiteSpace(word))
            {
                ViewBag.Error = "يرجى إدخال كلمة للبحث عن مرادفاتها.";
                var stories = await _context.Stories.ToListAsync();
                return View("Index", stories);
            }

            try
            {
                var synonyms = await _synonymsService.GetSynonymsAsync(word);

                ViewBag.Word = word;
                ViewBag.Synonyms = synonyms;

                var stories = await _context.Stories.ToListAsync();

                return View("Index", stories);
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"حدث خطأ أثناء الحصول على المرادفات: {ex.Message}";
                var stories = await _context.Stories.ToListAsync();
                return View("Index", stories);
            }
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var story = await _context.Stories
                .FirstOrDefaultAsync(m => m.Id == id);
            if (story == null)
            {
                return NotFound();
            }

            return View(story);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var story = await _context.Stories.FindAsync(id);
            if (story != null)
            {
                _context.Stories.Remove(story);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
