reddit-bot-public
=================
Hi! This is a quick bot designed for short-term moderation tasks that come about as a result of something happening that causes a mass of posts on a single topic.
(The original motivation is server down/up threads on r/leagueoflegends)

Its backend is based off of a lightly modified version of https://github.com/pressf12/reddit

I don't entirely know how this git thing works, I historically have used a local copy of subversion for all my source control needs, but the guts needed to run the thing are in the zipfile here at the project root.

Once you've downloaded it, there are a couple things to do.

1) Open Firefighter.exe.config in your favorite text editor, and edit settings as needed

	-markAsSpam: True to remove spam, false to remove ham

	-searchSelfText: If false, only titles will be searched.

	-replyWithComment: Enables responding to removed posts with a pre-defined comment, specified by the contents ffReply.ini

	-sleepDuration: How long, in seconds, to wait between cycles. Minimum suggested value is 10. 

	-targetSubreddit: The subreddit this will be run in. Case irrelevant (Theoretically), don't include /r/.

	-username, password: Moderator account in targetSubreddit's information

	-regexListFile, replyMarkupFile: Relative path to the respective files. No reason to change unless you're making multiple files to store settings.

2) Add regular expressions to match for removal to ffRegex.ini

	-One per line
	
	-Lines beginning with # are ignored
	
	-Whitespace at either end of the line is ignored
	
3) If using replyWithComment, put the reddit markup in ffReply.ini

	-This exact markup will be posted as a comment to any removed threads.
	
4) If you manage to break it, pm /u/p00rleno and I'll try to fix it. Or fix it yourself and tell me how. Either way is cool with me

I'm not sure how this will work in the long term. If anyone uses it for an extended period of time let me know how it works.


Copyright & License
---------------------

Copyright 2013 p00rleno

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this work except in compliance with the License.
You may obtain a copy of the License in the LICENSE file, or at:

   http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the Licens
