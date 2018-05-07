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
        public async Task<IActionResult> Create([Bind("ID,ProjectName,Details")] Project project)
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
        public async Task<IActionResult> Edit(string id, [Bind("ID,ProjectName,Details")] Project project)
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



            //CLIENTS
            var clients = await _context.Clients.ToListAsync();
            var projectroster = await _context.ProjectRoster.ToListAsync();            

            List<SelectListItem> clientsSelectList = new List<SelectListItem>();

            foreach(var client in clients)
            {
                clientsSelectList.Add(new SelectListItem { Value=client.ID, Text = client.FirstName + " " + client.LastName});
            }

            //MEMBERS
            var members = await _context.Members.ToListAsync();

            List<SelectListItem> membersSelectList = new List<SelectListItem>();
            var membersforedit = 
                from participant in _context.Members
                join projectparticipant in _context.ProjectRoster
                on participant.ID equals projectparticipant.ProjectParticipantID
                where project.ID == projectparticipant.ProjectID
                select participant;
            int indx = -1;

            foreach(var member in members)
            {
                indx++;
                membersSelectList.Add(new SelectListItem { Value=member.ID, Text = member.FirstName + " " + member.LastName});
                foreach(var item in membersforedit){
                    if(membersSelectList.Any(d => d.Value == item.ID)){
                        membersSelectList.RemoveAt(indx);
                        indx--;
                    }
                }

            }
            
            var clientsforedit = 
                from participant in _context.Clients
                join projectparticipant in _context.ProjectRoster
                on participant.ID equals projectparticipant.ProjectParticipantID
                where project.ID == projectparticipant.ProjectID
                select participant;


            
            
            //create and prepare ViewModel
            EditProjectDetailViewModel epdvm = new EditProjectDetailViewModel
            {
                TheProject = project,
                ProjectClients = clientsforedit.ToList() ?? null,
                ProjectClientsList = clientsSelectList,
                ProjectMembersList = membersSelectList
            };
            

            return View(epdvm);
        }        
        [HttpPost]
        public async Task<IActionResult> EditProjectParticipants(String id, EditProjectDetailViewModel model){
            var membersFromDb = _context.Members.ToList();
            Member ans = null;
            foreach(var items in membersFromDb){
                if(items.ID == model.SelectedID){
                    ans = items;
                }
            }
            var projectroster = new ProjectRoster {
                    ProjectID = model.TheProject.ID.ToString(), 
                    Project = model.TheProject,
                    ProjectParticipantID = model.SelectedID.ToString(),
                    ProjectParticipant = ans};
            _context.ProjectRoster.Add(projectroster);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
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