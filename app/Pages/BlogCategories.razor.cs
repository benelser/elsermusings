using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using app.Shared;

namespace app.Pages
{
    public class BlogCategoriesBase : ComponentBase
    {
        
        [Parameter]
        public string Category { get; set; }
        [Parameter]
        public string Title { get; set; }
        [Parameter]
        public string UUID { get; set; }
        [Parameter]
        public EventCallback OnClickEvent { get; set; }
        public bool BlogSelected { get; set; } = false;
        protected BlogBase SelectedBlog { get; set; } = new BlogBase();
        public int TotalNumberOfBlogs { get; set; } 
        protected List<BlogBase> FetchedBlogs { get; set; } = new List<BlogBase>();
        private List<BlogBase> InitialBlogs { get; set; } = new List<BlogBase>();

        protected override void OnInitialized()
        {
            
            Console.WriteLine("Inside init getting first batch of blogs");
            TotalNumberOfBlogs = BlogBase.GetTotalNumberOfBlogBases(Category);
            List<BlogBase> InitialBlogs = BlogBase.ReadBlogBasesFromDatabase(Category, 10, 0);
            foreach (BlogBase b in InitialBlogs)
            {
                FetchedBlogs.Add(b);
            }
            Console.WriteLine($"Blogs = {this.FetchedBlogs.Count}");
        }

        static async Task<string> GetObjectFromDataBase()
        {
            var message = await Task.FromResult<string>("Reading database");
            return message;
        }

        protected override async Task OnAfterRenderAsync(bool firstRender) 
        {
            if (firstRender)
            {
                // Set things up here
                Console.WriteLine(await GetObjectFromDataBase());
            }
            
            Console.WriteLine("Inside OnAfterRenderAsync");
        
        }
        public void OnSelectedBlog(BlogBase b)
        {
            BlogSelected = true;
            SelectedBlog = b;
            System.Console.WriteLine(SelectedBlog);
        }

        public async Task LoadMoreBlogs()
        {
            Console.WriteLine("Fetching more blogs");
            await OnClickEvent.InvokeAsync(this); // trigger eventcallback passing object if needed using <T>
            // This then triggers OnAfterRenderAsync method to be invoked;
            GetBlogsByCategory();

        }

        protected List<BlogBase> GetBlogsByCategory()
        {
            List<BlogBase> MoreBlogs = BlogBase.ReadBlogBasesFromDatabase(Category, 10, (FetchedBlogs.Count - 1));
            foreach (BlogBase b in MoreBlogs)
            {
                FetchedBlogs.Add(b);
            }
            return MoreBlogs;
        }
    }
}