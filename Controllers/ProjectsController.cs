using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BuffteksWebsite.Models;

namespace BuffteksWebsite.Controllers
{
    public class ProjectsController : Controller
    {
        private readonly BuffteksWebsiteContext _context;

        public ProjectsController(BuffteksWebsiteContext context)
        {
            _context = context;
        }

        // GET: Projects
        public async Task<IActionResult> Index()
        {
            return View(await _context.Projects.ToListAsync());
        }

        // GET: Projects/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Projects
                .SingleOrDefaultAsync(m => m.ID == id);

            if (project == null)
            {
                return NotFound();
            }

            var clients = 
                from participant in _context.Clients
                join projectparticipant in _context.ProjectRoster
                on participant.ID equals projectparticipant.ProjectParticipantID
                where project.ID == projectparticipant.ProjectID
                select participant;

            var members = 
                from participant in _context.Members
                join projectparticipant in _context.ProjectRoster
                on participant.ID equals projectparticipant.ProjectParticipantID
                where project.ID == projectparticipant.ProjectID                
                select participant;

            ProjectDetailViewModel pdvm = new ProjectDetailViewModel
            {
                TheProject = project,
                ProjectClients = clients.ToList() ?? null,
                ProjectMembers = members.ToList() ?? null
            };


            return View(pdvm);
        }

        // GET: Projects/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Projects/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,ProjectName,ProjectDescription")] Project project)
        {
            if (ModelState.IsValid)
            {
                _context.Add(project);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(project);
        }

        // GET: Projects/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Projects.SingleOrDefaultAsync(m => m.ID == id);
            if (project == null)
            {
                return NotFound();
            }
            return View(project);
        }

        // POST: Projects/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("ID,ProjectName,ProjectDescription")] Project project)
        {
            if (id != project.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(project);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProjectExists(project.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(project);
        }

        // GET: Projects/EditProjectParticipants/5
        public async Task<IActionResult> EditProjectParticipants(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Projects.SingleOrDefaultAsync(m => m.ID == id);
            if (project == null)
            {
                return NotFound();
            }


            //var clients = await _context.Clients.ToListAsync();

            //CLIENTS
            //pull 'em into lists first
            var clients = await _context.Clients.ToListAsync();
            var projectroster = await _context.ProjectRoster.ToListAsync();


            var uniqueclients = 
                from participant in clients
                join projectparticipant in projectroster
                on participant.ID equals projectparticipant.ProjectParticipantID
                where participant.ID != projectparticipant.ProjectParticipantID
                select participant;

            List<SelectListItem> clientsSelectList = new List<SelectListItem>();

            foreach(var client in clients)
            {
                clientsSelectList.Add(new SelectListItem { Value=client.ID, Text = client.FirstName + " " + client.LastName});
            }




            var membersOnProject = 
                from participant in _context.Members
                join projectparticipant in _context.ProjectRoster
                on participant.ID equals projectparticipant.ProjectParticipantID
                where project.ID == projectparticipant.ProjectID                
                select participant;

            var allMembers = 
                from participant in _context.Members           
                select participant;              

            var allMembersList = allMembers.ToList();
            var membersOnProjectList = membersOnProject.ToList();

            var membersNotOnProject = allMembersList.Except(membersOnProjectList).ToList();
            
            List<SelectListItem> membersSelectList = new List<SelectListItem>();

            foreach(var member in membersNotOnProject)
            {
                membersSelectList.Add(new SelectListItem { Value=member.ID, Text = member.FirstName + " " + member.LastName});
            }

            //create and prepare ViewModel
            EditProjectDetailViewModel epdvm = new EditProjectDetailViewModel
            {
                TheProject = project,
                ProjectClientsList = clientsSelectList,
                ProjectMembersList = membersSelectList
            };
           
           
            return View(epdvm);
        }        
        // POST: Projects/EditProjectParticipants/5
        // [HttpPost, ActionName("EditProjectParticipants")]
        // [ValidateAntiForgeryToken]
        // public async Task<IActionResult> AddConfirmed(string id)
        // {
            
        //     var participant = EditProjectDetailViewModel.ProjectMembersList(m => m.ID == id);
        //     _context.ProjectRoster.Add(participant);
        //     await _context.SaveChangesAsync();
        //     return RedirectToAction(nameof(Index));

            
            
        // }


        // GET: Projects/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Projects
                .SingleOrDefaultAsync(m => m.ID == id);
            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // POST: Projects/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var project = await _context.Projects.SingleOrDefaultAsync(m => m.ID == id);
            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProjectExists(string id)
        {
            return _context.Projects.Any(e => e.ID == id);
        }
    }
}