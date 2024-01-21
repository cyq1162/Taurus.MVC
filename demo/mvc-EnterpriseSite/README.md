# Taurus.MVC.Demo
Taurus.MVC 的Demo，企业站示例（Demo for Taurus.MVC)
<h1><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">Foreword:</span></span></h1>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">I was planning to write an article called: Taurus.MVC. From entry to mastery, a complete story!</span></span></p>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">Later, I turned to a thought, or set the tutorial on this enterprise station project! </span><span style="vertical-align: inherit;">! </span><span style="vertical-align: inherit;">!</span></span></p>
<h1><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">Enterprise station style:</span></span></h1>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">I have previously sent a business station written by my sisters: I </span></span><a id="cb_post_title_url" class="postTitle2" href="http://www.cnblogs.com/cyq1162/p/3573726.html"><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">spent a few nights recently helping my sisters complete a corporate website.</span></span></a></p>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">Technical style is: text database (txt) + WebForm</span></span></p>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">Transformation style: text database (txt) + Taurus.MVC</span></span></p>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">The interface to be completed today is mainly the home page:</span></span></p>
<p><img src="http://images2015.cnblogs.com/blog/17408/201608/17408-20160815211949093-179801402.jpg" alt="" /></p>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">OK, let's get started, take a look at how to do this business project from scratch:</span></span></p>
<h1><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">1: Create a new ASP.NET empty web application project:</span></span></h1>
<p><img src="http://images2015.cnblogs.com/blog/17408/201608/17408-20160815212221343-89583303.jpg" alt="" /></p>
<h1><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">2: Add a class library project called Taurus.Controllers</span></span></h1>
<p><img src="http://images2015.cnblogs.com/blog/17408/201608/17408-20160815212413359-851128632.jpg" alt="" /></p>
<h1><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">3: Use Tauget to reference Taurus.MVC on the Taurus.Controllers project:</span></span></h1>
<p><img src="http://images2015.cnblogs.com/blog/17408/201608/17408-20160815213555546-101558165.jpg" alt="" /></p>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">Of course, you can also use the source project, or find two DLLs in the source to add references:</span></span></p>
<p><img src="http://images2015.cnblogs.com/blog/17408/201608/17408-20160815213751406-969399371.jpg" alt="" /></p>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">Note that the EnterPriseSite project needs to reference the Taurus.Controllers project. After all, the compiled DLLs are concentrated on the EnterPriseSite project.</span></span></p>
<h1><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">4: Add the Views folder on the EnterPriseSite project, and several empty pages:</span></span></h1>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">The default.html and other pages are not placed in the default folder, it is wrong, the screenshots will be corrected.</span></span></p>
<p><img src="http://images2015.cnblogs.com/blog/17408/201608/17408-20160815214134140-1985750123.jpg" alt="" /></p>
<h1><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">5: Copy the text database under the Style and App_Data directories from the original WebForm project:</span></span></h1>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">By the way, change the project name to EnterPriseSite.View</span></span></p>
<p><img src="http://images2015.cnblogs.com/blog/17408/201608/17408-20160815214438125-426006235.jpg" alt="" /></p>
<h1><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">6: Copy the html tag of the user control of the original project and put it in the master.html in the Shared directory:</span></span></h1>
<p><img src="http://images2015.cnblogs.com/blog/17408/201608/17408-20160815214545390-1644609426.jpg" alt="" /><img src="http://images2015.cnblogs.com/blog/17408/201608/17408-20160815214700640-795341470.jpg" alt="" /></p>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">When using Taurus.MVC, there is a concept of Repeater, only Html and JS.</span></span></p>
<p><strong><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">If there is a business condition code, it can be processed by JS or in the background. Here, it is processed by Js (there is a demonstration background processing):</span></span></strong></p>
<p><img src="http://images2015.cnblogs.com/blog/17408/201608/17408-20160815214804093-847679335.jpg" alt="" /></p>
<p><strong><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">The nodes of the template page can be placed casually. As long as the node has a name (id or name), it can be referenced by other html.</span></span></strong></p>
<p><strong><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">The referenced attribute name can be id, name or individual tags such as (head, body, title, script, style, form, meta, link)</span></span></strong></p>
<p><strong><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">So you can use master.head, master, body, master.title, master.script (all take the first node)...</span></span></strong></p>
<h1><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">7: Copy the label of the Default page from the original interface:</span></span></h1>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">&nbsp;The original Default.aspx:</span></span></p>
<p><img src="http://images2015.cnblogs.com/blog/17408/201608/17408-20160815215818406-1327502485.jpg" alt="" /></p>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">Now Default.html</span></span></p>
<p><img src="http://images2015.cnblogs.com/blog/17408/201608/17408-20160815215841250-1921045925.jpg" alt="" /></p>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">The interface is all done, now I have to write the code:</span></span></p>
<h1><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">8: Create a new DefaultController.cs, and copy several table classes of CodeFirst:</span></span></h1>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">I got a Logic folder to put it. When the project is simple, I don't want to build too many projects. I use folders:</span></span></p>
<p><img src="http://images2015.cnblogs.com/blog/17408/201608/17408-20160815220107859-2125175894.jpg" alt="" /></p>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">Then, write a few methods up so that you can load into the corresponding html file:</span></span></p>
<p><img src="http://images2015.cnblogs.com/blog/17408/201608/17408-20160815220445984-501237012.jpg" alt="" /></p>
<p><strong><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">So html is placed in the /views/default/ directory.</span></span></strong></p>
<p><strong><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">Additional instructions:</span></span></strong></p>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">The default access path is: localhost/default/index, locahost/default/artilelist...</span></span></p>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">In order to remove the default, the default route I added one; the previous route mode was only 1 and 2, and now there is a 0.</span></span></p>
<h1><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">9: Look at the Web.Config configuration:</span></span></h1>
<p><img src="http://images2015.cnblogs.com/blog/17408/201608/17408-20160815221309765-1406093337.jpg" alt="" /></p>
<h1><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">10: Write the logic code binding page:</span></span></h1>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">Because the project is simple, I built a class directly into the business logic and threw it inside the Controller, called DefaultLogic.</span></span></p>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">In order to make the layering clear, the students still create a new Taurus.Logic class library project.</span></span></p>
<p><strong><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">The logic class needs to inherit from Taurus.Core.LogicBase, in order to pass the View object to the logic class, pay attention to the constructor.</span></span></strong></p>
<p><img src="http://images2015.cnblogs.com/blog/17408/201608/17408-20160815221615875-582326010.jpg" alt="" /></p>
<p><strong><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">Since the node id of the html is by convention: the table name View, Bind(View) is fine, and no name is required.</span></span></strong></p>
<p><img src="http://images2015.cnblogs.com/blog/17408/201608/17408-20160815221718734-1356398969.jpg" alt="" /></p>
<p><strong><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">Here is an event View.Onforeach that is used to format the time the interface is rendered:</span></span></strong></p>
<p><img src="http://images2015.cnblogs.com/blog/17408/201608/17408-20160815221852250-898178364.jpg" alt="" /></p>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">Of course, the background is not formatted first, then the foreground is processed, just like one of the screenshots above is the code processed by JS.</span></span></p>
<h1><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">11: The Controller calls the logic code to render the page:</span></span></h1>
<p><img src="http://images2015.cnblogs.com/blog/17408/201608/17408-20160815222335109-1346588678.jpg" alt="" /></p>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">Finally, a beautiful home page came out:</span></span></p>
<p><img src="http://images2015.cnblogs.com/blog/17408/201608/17408-20160815222739859-1422879440.jpg" alt="" /></p>
<h1><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">After finishing the work:</span></span></h1>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">The following pages, including article list, article details, product center, and background management, will be introduced in the next article.</span></span></p>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">In the past, the whole project was finished and then the article was written. Now the project is written in half, and the article is supplemented with one article. </span><span style="vertical-align: inherit;">. </span><span style="vertical-align: inherit;">.</span></span></p>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">Source code:</span></span></p>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">The URL opens: </span></span><a href="http://code.taobao.org/p/cyqopen/src/trunk/Taurus.MVC.GettingStarted/" target="_blank"><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">http://code.taobao.org/p/cyqopen/src/trunk/Taurus.MVC.GettingStarted/</span></span></a></p>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">Svn checkout: http://code.taobao.org/svn/cyqopen/trunk/Taurus.MVC.GettingStarted</span></span></p>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">Thank you for your support! </span><span style="vertical-align: inherit;">! </span><span style="vertical-align: inherit;">!</span></span></p>

-----------------------------------------------

<h1><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">Foreword:</span></span></h1>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">The previous one completed the home page, and this one gave the remaining functions.</span></span></p>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">Includes article lists, article details, and product displays.</span></span></p>
<h1><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">1: Article list:</span></span></h1>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">Original ArticleList.aspx</span></span></p>
<p><img src="http://images2015.cnblogs.com/blog/17408/201608/17408-20160816203332781-270074002.jpg" alt="" /></p>
<h2><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">1: Current articlelist.html</span></span></h2>
<p><img src="http://images2015.cnblogs.com/blog/17408/201608/17408-20160816203350187-1942107720.jpg" alt="" /></p>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">In addition to the shared module, there is just one more list display, and the total number of records (I even save the page...)</span></span></p>
<h2><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">2: Next is the logic code that binds the articleView:</span></span></h2>
<p><img src="http://images2015.cnblogs.com/blog/17408/201608/17408-20160816203546421-1292866477.jpg" alt="" /></p>
<h2><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">3: Finally, the Controller calls:</span></span></h2>
<p><img src="http://images2015.cnblogs.com/blog/17408/201608/17408-20160816203657625-1675700987.jpg" alt="" /></p>
<h3><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">Then a list page is complete:</span></span></h3>
<p><img src="http://images2015.cnblogs.com/blog/17408/201608/17408-20160816203759578-709417068.jpg" alt="" /></p>
<h1><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">2: Article details page:</span></span></h1>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">Original ArticleDetail.aspx</span></span></p>
<p><img src="http://images2015.cnblogs.com/blog/17408/201608/17408-20160816203939359-1821584087.jpg" alt="" /></p>
<h2><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">1: now articledetail.html</span></span></h2>
<p><img src="http://images2015.cnblogs.com/blog/17408/201608/17408-20160816204103906-802532972.jpg" alt="" /></p>
<h2><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">2: Logic code:</span></span></h2>
<p><img src="http://images2015.cnblogs.com/blog/17408/201608/17408-20160816204202078-223943601.jpg" alt="" /></p>
<h2><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">3: Controller calls the logic:</span></span></h2>
<p><img src="http://images2015.cnblogs.com/blog/17408/201608/17408-20160816204233812-2118118143.jpg" alt="" /></p>
<h3><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">Then, the details page is done:</span></span></h3>
<p><img src="http://images2015.cnblogs.com/blog/17408/201608/17408-20160816204554140-1644331755.jpg" alt="" /></p>
<h1><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">3: Product Center:</span></span></h1>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">The original PhotoList.aspx:</span></span></p>
<p><img src="http://images2015.cnblogs.com/blog/17408/201608/17408-20160816204741187-85065889.jpg" alt="" /></p>
<h2><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">1: Now photolist.html:</span></span></h2>
<p><img src="http://images2015.cnblogs.com/blog/17408/201608/17408-20160816204902234-16718124.jpg" alt="" /></p>
<h2><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">2: background logic code</span></span></h2>
<p><img src="http://images2015.cnblogs.com/blog/17408/201608/17408-20160816205033875-108538.jpg" alt="" /></p>
<h2><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">3: Controller calls:</span></span></h2>
<p><img src="http://images2015.cnblogs.com/blog/17408/201608/17408-20160816205044718-1964919822.jpg" alt="" /></p>
<h3><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">Then the interface comes out (the author is too lazy, the thumbnail is not used when the change is made, so the picture below is not good after compression):</span></span></h3>
<p><img src="http://images2015.cnblogs.com/blog/17408/201608/17408-20160816205203062-1435658490.jpg" alt="" /></p>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">Click on the image to enlarge the effect:</span></span></p>
<p><img src="http://images2015.cnblogs.com/blog/17408/201608/17408-20160816205307437-1151069736.jpg" alt="" /></p>
<h1><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">Source address:</span></span></h1>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">The URL opens: </span></span><a href="http://code.taobao.org/p/cyqopen/src/trunk/Taurus.MVC.GettingStarted/" target="_blank"><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">http://code.taobao.org/p/cyqopen/src/trunk/Taurus.MVC.GettingStarted/</span></span></a></p>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">SVN CheckOut: http://code.taobao.org/svn/cyqopen/trunk/Taurus.MVC.GettingStarted</span></span></p>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">Before the first half of the previous section, the direct SVN update is fine.</span></span></p>
<h1><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">to sum up:</span></span></h1>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">At this point, a business station will be finished two or three times, but the user backstage? </span><span style="vertical-align: inherit;">? </span><span style="vertical-align: inherit;">?</span></span></p>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">The user backstage, temporarily no plans to get it, everyone should configure a few links with the Aries framework should be fine.</span></span></p>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">Does Aries support text databases? </span><span style="vertical-align: inherit;">? </span><span style="vertical-align: inherit;">? </span><span style="vertical-align: inherit;">(In fact, I don't know, I haven't tested it, haha)</span></span></p>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">Overall, the following points of Taurus.MVC are quite obvious:</span></span></p>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">1: Separation of the front and rear ends.</span></span></p>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">2: html is simple, no background code intrusion.</span></span></p>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">3: The background code is also very simple.</span></span></p>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">4: The whole is very light.</span></span></p>
<h1><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">Someone asked how the framework was designed?</span></span></h1>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">1: Writing a framework is a process of continuous accumulation and improvement. </span><span style="vertical-align: inherit;">(No one can go to the sky one step at a time, write an excellent framework, except for B).</span></span></p>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">2: Practice is the only truth to test the framework (if the framework you write is not used for a while, use it yourself and use it as much as possible on different systems).</span></span></p>
<p><strong><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">In short: the framework is not designed, the framework is reconstructed on the basis of the previous accumulation! </span><span style="vertical-align: inherit;">! </span><span style="vertical-align: inherit;">!</span></span></strong></p>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">In several of my frameworks:</span></span></p>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">CYQ.Data: It has a history of 10 years. It is not necessary to talk about the stability and function. It has been charged for a while.</span></span></p>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">ASP.NET Aries: There are also 2-3 years of history. The 1.0 version has been used in previous companies, and dozens of projects have been applied. Although 2.0 has been almost completely rewritten, the overall stability tends to be stable.</span></span></p>
<p><span style="vertical-align: inherit;"><span style="vertical-align: inherit;">Taurus.MVC: It has only come out soon, and it is still very young. With the increase in the number of people and business scenarios, I believe that there will be a lot of room for evolution in the future.</span></span></p>
