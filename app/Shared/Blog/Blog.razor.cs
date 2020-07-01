using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using System.Linq;
using Microsoft.AspNetCore.Components.Routing;
// using System.Text.RegularExpressions;

namespace app.Shared
{
    
    public class BlogBase : ComponentBase
    {
        // Each component is it's own class
        [Parameter]
        public string Title { get; set; }
        [Parameter]
        public string Author { get; set; } 
        [Parameter]
        public string DatePublished { get; set; }
        [Parameter]
        public string Body { get; set; }
        [Parameter]
        public string Category { get; set; }
        [Parameter]
        public bool Trim { get; set; }
        [Parameter]
        public bool SingleRender { get; set; }
        [Parameter]
        public EventCallback<BlogBase> OnClickEvent { get; set; }
        [Parameter]
        public string UUID { get; set; }
        private MarkupString BodyString { get; set;}
        public string TrimedBody { get; set; }
        public string href { get; set; }
        [Inject]
        private NavigationManager NavManager { get; set;}

        // For static testing
        private static string Introduction = $@"
    This is a test of writing html and feeding it to my component.
        ";
        private static string BodyBlob = $@"
    <h2>Why painful the sixteen how minuter looking nor</h2>
    <p>In as name to here <strong>This text is important!</strong> them deny wise this. As rapid woody my he me which. Men but they fail shew just wish next put. Led all visitor musical calling nor her. Within coming figure sex things are. Pretended concluded did repulsive education smallness yet yet described. Had country man his pressed shewing. No gate dare rose he. Eyes year if miss he as upon.</p>
    <p>He went such dare good mr fact. The small own seven saved <strong>This text is important!</strong> man age ﻿no offer. Suspicion did mrs nor furniture smallness. Scale whole downs often leave not eat. An expression reasonably cultivated indulgence mr he surrounded instrument. Gentleman eat and consisted are pronounce distrusts.</p>
    <p>And produce say the ten moments parties. Simple innate summer fat appear basket his desire joy. Outward clothes promise at gravity do excited. Sufficient particular impossible by reasonable oh expression is. Yet preference connection unpleasant yet melancholy but end appearance. And excellence partiality estimating terminated day everything.</p>
    <p>Able an hope of body. <strong>This text is important!</strong> Any nay shyness article matters own removal nothing his forming. Gay own additions education satisfied the perpetual. If he cause manor happy. Without farther she exposed saw man led. Along on happy could cease green oh.</p>
    <p>Much evil soon high in hope do view. Out may few northward believing attempted. Yet timed being songs marry one defer men our. Although finished blessing do of. Consider speaking me prospect whatever if. Ten nearer rather hunted six parish indeed number. Allowance repulsive sex may contained can set suspected abilities cordially. Do part am he high rest that. So fruit to ready it being views match.</p>
    <p>Talent she for lively eat led sister. Entrance strongly packages she out rendered get quitting denoting led. Dwelling confined improved it he no doubtful raptures. Several carried through an of up attempt gravity. Situation to be at offending elsewhere distrusts if. Particular use for considered projection cultivated. Worth of do doubt shall it their. Extensive existence up me contained he pronounce do. Excellence inquietude assistance precaution any impression man sufficient.</p>
    <p>Improved own provided blessing may peculiar domestic. Sight house has sex never. No visited raising gravity outward subject my cottage mr be. Hold do at tore in park feet near my case. Invitation at understood occasional sentiments insipidity inhabiting in. Off melancholy alteration principles old. Is do speedily kindness properly oh. Respect article painted cottage he is offices parlors.</p>
    <p>Depart do be so he enough talent. Sociable formerly six but handsome. Up do view time they shot. He concluded disposing provision by questions as situation. Its estimating are motionless day sentiments end. Calling an imagine at forbade. At name no an what like spot. Pressed my by do affixed he studied.</p>
    <p>Lose eyes get fat shew. Winter can indeed letter oppose way change tended now. So is improve my charmed picture exposed adapted demands. Received had end produced prepared diverted strictly off man branched. Known ye money so large decay voice there to. Preserved be mr cordially incommode as an. He doors quick child an point at. Had share vexed front least style off why him.</p>
    ";

        protected override void OnInitialized()
        {
            this.BodyString = ((MarkupString)this.Body);
            this.href = $"Blog/{Category}/{Title}/{UUID}";
            this.TrimBody(75);
            NavManager.LocationChanged += LocationChanged;
            if (this.Trim == false)
            {
                Console.WriteLine("Inside Oninit");
            }
            base.OnInitialized();

        }

        protected override void OnParametersSet()
        {
            if (this.Trim == false)
            {
                Console.WriteLine("Inside param set");
            }
        }

        void LocationChanged(object sender, LocationChangedEventArgs e)
        {
            
            if (this.Trim == false)
            {
                Console.WriteLine("Inside local change");
            }
            
        }

        void Dispose()
        {
            if (Trim == false)
            {
                Console.WriteLine("Inside dispose");
            }
            // this.Trim = true;
            // this.SingleRender = false;
            // NavManager.LocationChanged -= LocationChanged;
        }
 
        public BlogBase()
        {
            
        }

        public BlogBase(string title, string author, string datepublished, string body, string category, string uuid)
        {
            this.Title = title;
            this.Author = author;
            this.DatePublished = datepublished;
            this.Body = body;
            this.Category = category;
            this.UUID = uuid;
        }

        private void TrimBody(int wordamount)
        {
            string[] words = this.BodyString.ToString().Split(" ");
            StringBuilder sb = new StringBuilder();
            sb.AppendJoin(" ", words.Take(wordamount));
            sb.Append("...");
            //string htmltagspatter = "<[^>]*>";
            //this.TrimedBody = Regex.Replace(sb.ToString(), htmltagspatter, String.Empty);
            this.TrimedBody = sb.ToString();
            
        }

        public override string ToString()
        {
            return $"href: {href} TrimState: {this.Trim} SingleRender: {this.SingleRender} Title: {this.Title} Author: {this.Author} Date: {this.DatePublished} Category: {this.Category}";
        }


        public async Task GetSelectedBlog()
        {
            Console.WriteLine("BlogBase Selected");
            this.Trim = false;
            await OnClickEvent.InvokeAsync(this);

        }

    // Demo read operation
        public static List<BlogBase> ReadBlogBasesFromDatabase(string category, int amount, int index)
        {
            Console.WriteLine($"Getting BlogBases starting at index {index}");
            List<BlogBase> BlogBases = new List<BlogBase>();
            for (int i = 0; i < amount; i++)
            {
                string g = Guid.NewGuid().ToString();
                BlogBase b = new BlogBase($"Super Enlightening Blog{i}","Benjamin Elser", DateTime.Today.ToShortDateString(), BodyBlob, category, g);
                BlogBases.Add(b);
            }
        
            return BlogBases;
            
        }

        public static BlogBase GetSpecificBlogBaseByUUID(string uuid)
        {
            Console.WriteLine("Fetching BlogBase via primary key UUID");
            return new BlogBase($"Test","Ben", DateTime.Today.ToShortDateString(), BodyBlob, "python", "12345-5678");
        }

        public static int GetTotalNumberOfBlogBases(string category)
        {
            Console.WriteLine($"getting total number of BlogBases for {category}");
            Console.WriteLine($"Total number of BlogBases for {category} is 22");
            return 22;
        }

    }
}