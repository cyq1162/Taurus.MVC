# Taurus.MVC
Taurus.mvc is a high-performance mvc and webapi framework for asp.net or asp.net core（适合场景：对性能和并发有较高要求的电商、站点、WebAPI等系统，支持.Net Core）,created by Aster（路过秋天）<hr />

QQ交流群：6033006<br />

Windows 部署：http://taurus.cyqdata.com/ <br />
Linux（CentOS7) 部署：http://mvc.taurus.cyqdata.com
<hr />
Demo：https://github.com/cyq1162/Taurus.MVC.Demo <br />

<hr />
<h1><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">Why create Taurus.MVC:</span></span></h1>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">I remember when I was fooled by the last company to take charge of the company's e-commerce platform, the situation is this:</span></span></p>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">The original version of the project is outsourced to a third party. Using: WebForm+NHibernate, the code is unsightly, the bug is infinite, and it is often hung up.</span></span></p>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">At the beginning, I recruited a few internship college students to play there. I couldn&rsquo;t make it anymore, and finally I was fooled, haha. </span><span style="vertical-align: inherit;">. </span><span style="vertical-align: inherit;">.</span></span></p>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">The first feeling that I went in at the time was to redo, but hehe, the boss&rsquo;s mind can&rsquo;t guess.</span></span></p>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">Then the first stage is to maintain the stability of the old project. As long as it is not a problem that needs to be solved by hundreds of servers, it can be handled weakly. After all, there are no three or two, and it is not good for the beam.</span></span></p>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">In the second stage, nature is thinking about redoing:</span></span></p>
<p><strong><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">The e-commerce background has been open source: </span></span><a href="https://github.com/cyq1162/Aries" target="_blank"><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">ASP.NET Aries</span></span></a><span style="vertical-align: inherit;"><span style="vertical-align: inherit;"> framework (supported .NET Core), don't worry too much about brushing;</span></span></strong></p>
<h2><span style="color: #ff0000;"><strong><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">What framework does the e-commerce front desk choose?</span></span></strong></span></h2>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">1: WebForm is too conservative;</span></span></p>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">2: .NET Core 1.1 is too aggressive (now Taurus.MVC already supports .NET Core);</span></span></p>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">3: QBlog (autumn garden) threshold is high;</span></span></p>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">4: Re-write a set, busy business, no time to calm down and think, and time is limited, has submitted a plan to BOSS.</span></span></p>
<p><strong><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">Finally, only helplessly choose: ASP.NET MVC.</span></span></strong></p>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">Think about the .NET environment, the popular development framework on the market, all of Microsoft's own (say a good flower?)</span></span></p>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">I also know that some older people also make frameworks, but they are all made for themselves or their own companies (and the angle of thinking and the breadth involved are not the same).</span></span></p>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">There are also some free gifts to the people, but there is no sound in propaganda after three or two;</span></span></p>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">The garden has never taken the initiative to help third-party open source frameworks to promote, relying on the blogger's own passion and sentiment, how long it can support is an unknown number, after all, the framework is no income.</span></span></p>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">A stroke of the pen:</span></span></p>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">Later, the boss fell down..... (tears rush ~~~).</span></span></p>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">Then, there is time to calm down and build a framework with emotions!</span></span></p>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">Finally, Taurus.MVC came out, and it was open source when it came out! </span><span style="vertical-align: inherit;">! </span><span style="vertical-align: inherit;">! </span><span style="vertical-align: inherit;">Open source! </span><span style="vertical-align: inherit;">! </span><span style="vertical-align: inherit;">open! </span><span style="vertical-align: inherit;">Three times.</span></span></p>
<h1><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">About the frame name: Taurus</span></span></h1>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">When I made CYQ.Data ten years ago, the name was not good (blame me), which led to the promotion of resistance.</span></span></p>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">So now, to create a new framework, you must have a good name, after all, you have to take a picture like: Qi Delong, Qi Dongqiang, Qi Delong Dongqiang is so loud and thorough.</span></span></p>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">Previously released a little: ASP.NET Aries business development framework: named: Aries (Aries, gentle with a little pride).</span></span></p>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">So thinking, is the continuation of the Aries series called: Aries.MVC?</span></span></p>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">still is. </span><span style="vertical-align: inherit;">. </span><span style="vertical-align: inherit;">. </span><span style="vertical-align: inherit;">Create a golden zodiac?</span></span></p>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">Then I checked the English words of the thirteen constellations and the eight planets and found that they were not satisfied. If the jumping name is obstructed, then the name should be named.</span></span></p>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">Taurus (Taurus), in fact, the final decision is the pronunciation of the word: off (very big-looking feeling, and full of imagination, the feeling of a little Mimi in the explosion).</span></span></p>
<p><img src="http://images2015.cnblogs.com/blog/17408/201608/17408-20160805023118559-795702157.jpg" alt="" /></p>
<h1><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">Applicable scenarios for the framework:</span></span></h1>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">Choosing a framework is a matter of learning for the master; it is only a choice for the novice.</span></span></p>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">When I was young, I was forced to choose only the framework created by Microsoft. Now, I became the creator:</span></span></p>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">CYQ.Data+Aries+Taurus can adapt to almost all business scenarios.</span></span></p>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">Already without ASP.NET WebForm, ASP.NET MVC.</span></span></p>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">However, it still remains inseparable from the ASP.NET platform.</span></span></p>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">As mentioned above:</span></span></p>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">1: ASP.NET Aries is suitable for rapid development of business systems and backgrounds.</span></span></p>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">2: Taurus.MVC is suitable for front-end systems and WebAPIs such as e-commerce with high performance requirements.</span></span></p>
<h1><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">About the advantages of the framework:</span></span></h1>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">Usually speaking, the advantage of the framework is that when you start to blow B, as long as the market slogan is loud, the product is not a problem as long as it is not weak.</span></span></p>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">What are the advantages of the framework? </span><span style="vertical-align: inherit;">Ordinary people ask this first, you want to blow my heart, blow my heart, only to return to you, and then silently download the source code to save the hard drive.</span></span></p>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">Because the market is basically a unified Microsoft world, so it is more to find Microsoft's MVC.</span></span></p>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">In fact, compared with .NET MVC, you can only say: one heaven, one underground.</span></span></p>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">MVC4 installed: 800M (do not figure out what is to be installed so much);</span></span></p>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">Taurus.MVC installed: 400K (Taurus.Core.dll + CYQ.Data).</span></span></p>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">Obviously: Microsoft has been doing additions for the past few years, did not want to do subtraction, has been doing innovation, did not want to be compatible, many products are big and big, people are reluctant.</span></span></p>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">Far away, talk about the advantages, let me think about it, let me think about it with silence...</span></span></p>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">Use a few words that are overused: lightweight? </span><span style="vertical-align: inherit;">high performance? </span><span style="vertical-align: inherit;">high efficiency?</span></span></p>
<p><strong><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">No, it&rsquo;s different, a little bit of what others don&rsquo;t do is called an advantage:</span></span></strong></p>
<p><strong><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">Oh, yes, you have to use a graph to show that you can be professional, yes, this way, that way, good, finished, above:</span></span></strong></p>
<p><strong><img src="http://images2015.cnblogs.com/blog/17408/201608/17408-20160805014733122-66381202.jpg" alt="" /></strong></p>
<h1><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">Taurus.MVC Source code:</span></span></h1>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">1: Source code SVN: </span></span><a href="https://github.com/cyq1162/Taurus.MVC"><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">https://github.com/cyq1162/Taurus.MVC</span></span></a></p>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">2: Demo demo station: </span></span><a href="http://taurus.cyqdata.com/"><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">http://taurus.cyqdata.com</span></span></a></p>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">The Demo screenshot is like this (the new version now has a WebAPI Demo):</span></span></p>
<p><img src="http://images2015.cnblogs.com/blog/17408/201608/17408-20160805023918043-2063699248.jpg" alt="" /></p>
<h1><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">Taurus.MVC framework introduction method:</span></span></h1>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">1: Search on Nuget: Taurus.MVC, reference (will be introduced: Taurus.Core and CYQ.Data)</span></span></p>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">Then come out a Readme.txt, follow the prompts to configure the URL interception and specify the dll of the Controller place.</span></span></p>
<p><strong><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">.NET Core version search: Taurus.MVC.Core</span></span></strong></p>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">2: Directly use the source project (there will be Demo in the source project).</span></span></p>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">.NET version running: Taurus.MVC.sln</span></span></p>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">.NET Core version running: Taurus.MVC_Core_VS2017.sln</span></span></p>
<h1><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">Introduction to the Taurus.MVC framework:</span></span></h1>
<h2><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">1: After downloading the source code: Solution diagram:</span></span></h2>
<p><img src="http://images2015.cnblogs.com/blog/17408/201608/17408-20160805005840497-1095313333.jpg" alt="" /></p>
<h2><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">2: Solution Description:</span></span></h2>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">1: CYQ.Data: The main XHtmlAction is the template engine, additionally when the data layer can provide a Model or provide automatic binding syntax.</span></span></p>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">2: Taurus.Core: Mainly implements core methods such as route rewriting, Controller calling, and ViewEngine.</span></span></p>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">3: Taurus.Controllers method entry, where to write the code.</span></span></p>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">4: Taurus.View only stores html and css and js</span></span></p>
<h2><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">3: Supplementary note:</span></span></h2>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">1: Usually MVC Controller, Modle, View files are placed in a project, here split into two projects.</span></span></p>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">2: In order to clear the project level, you can build Model project (put entity) and Logic project (write business logic code) and Utility (put tool class).</span></span></p>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">3: The Demo provided by the framework is fully loaded into the Controllers project.</span></span></p>
<p><strong><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">In the following, according to the MVC routine, simply explain the basic principles and usage:</span></span></strong></p>
<h1><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">1: Route of Taurus.MVC:</span></span></h1>
<h2><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">1: Hidden route:</span></span></h2>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">In .NET MVC, routing is a very important but cumbersome feature.</span></span></p>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">The first step in simplifying MVC is to think about how to implicitly eliminate routing.</span></span></p>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">Finally, the internal default has 3 routes:</span></span></p>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">0:{Action}/{Para}</span></span></p>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">1:{Controller}/{Action}/{Para}</span></span></p>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">2: {Module}/{Controller}/{Action}/{Para}</span></span></p>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">The default is 1.</span></span></p>
<h2><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">2: Extended routing:</span></span></h2>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">When deployed as a sub-application, or when the first one is a username, an additional prefix directory is created.</span></span></p>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">At this time, it is possible to configure the RouteMode value to 2 by AppSetting, which is easy and excessive.</span></span></p>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">The context provides three parameters for you to get information: ControllerType, Action, Para.</span></span></p>
<p><span style="color: #ff0000;"><strong><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">Ok, the routing is finished, want to customize the route? </span><span style="vertical-align: inherit;">Do some innovation on Para~~~~</span></span></strong></span></p>
<h1><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">2: Taurus.Controllers</span></span></h1>
<h2><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">1: Looking for the Controller:</span></span></h2>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">The rules have been fixed, and the rest is to find the Controller according to the rules.</span></span></p>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">1: Collect all the Controllers.</span></span></p>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">2: Specify where to collect: The default is to go to Taurus.Controllers to find inheritance from the base class: Taurus.Core.Controller.</span></span></p>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">3: Customize the Controllers: AppSetting to configure the value of Taurus.Controllers, assuming: Taurus.View</span></span></p>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">4: When you can't find the Controller, you can find the DefaultController. If this is all there is (there is some in Demo), it will throw an exception.</span></span></p>
<p><img src="http://images2015.cnblogs.com/blog/17408/201608/17408-20160805025506872-16659224.jpg" alt="" /></p>
<h2><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">2: Call the Controller's Action:</span></span></h2>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">1: The method names are all public void, which can have parameters (overloading multiple parameters, only the first one is collected by default).</span></span></p>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">2: If there is input, use the Write method.</span></span></p>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">3: When the Action is not found, the Default method will be found (this base class is there, so there must be, and it will be rewritten if necessary).</span></span></p>
<p><img src="http://images2015.cnblogs.com/blog/17408/201608/17408-20160805025353559-490764850.jpg" alt="" /></p>
<h1><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">3: Taurus.View</span></span></h1>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">1: Template: html (strictly speaking, it should be xhtml)</span></span></p>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">2: Template loading method: The addressing path corresponding to the URL: Views/{Controller}/{Action}.html, which can change the agreed path through configuration.</span></span></p>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">3: Reference method of master page: itemref="page.node name". </span><span style="vertical-align: inherit;">( </span></span><strong><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">itemref is the attribute of a div. If no one uses it, it will be used to reference node replacement</span></span></strong><span style="vertical-align: inherit;"><span style="vertical-align: inherit;"> .)</span></span></p>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">4: Load replacement syntax:</span></span></p>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">A: For the input tag, you can use CYQ.Data.MDataRow.SetToAll to assign values ​​in bulk.</span></span></p>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">B: For ${name}, you can use View.LoadData (data, "prefix"), which will be formatted automatically.</span></span></p>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">C: For list loop tags: you can bind using the CYQ.Data.MDataTable.Bind method.</span></span></p>
<p><img src="http://images2015.cnblogs.com/blog/17408/201608/17408-20160805025919762-770843444.jpg" alt="" /></p>
<h1><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">to sum up:</span></span></h1>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">1: This article does not explain the usage method in detail. For the usage, it will be introduced in the next article:</span></span></p>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">Well, an introduction is enough, because there is nothing to say, you don't need to write a book.</span></span></p>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">2: Demo provides the function of adding, deleting, and revising list paging. If the ability is good or has MVC basics, the source code will be used.</span></span></p>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">3: Today's focus is on open source. </span><span style="vertical-align: inherit;">. </span><span style="vertical-align: inherit;">. </span><span style="vertical-align: inherit;">Open source. </span><span style="vertical-align: inherit;">. </span><span style="vertical-align: inherit;">. </span><span style="vertical-align: inherit;">Open source. </span><span style="vertical-align: inherit;">. </span><span style="vertical-align: inherit;">. </span><span style="vertical-align: inherit;">The important thing is to say 123.</span></span></p>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">Finally say:</span></span></p>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">The open source of this framework gives the people of .NET a choice.</span></span></p>
