﻿#Multi Tenant Demo instructions

Note these instructions are not up to date and the startup code in this example is a bit different than it would typically be with a new project because this demo was created during the rc1 timeframe.
A better sample can be found here: https://github.com/joeaudette/cloudscribe.StarterKits/tree/master/SimpleContent-SimpleAuth-nodb-multitenant/src/WebApp

in windows explorer control-shift-right click the project folder "multiTenant.WebApp" and choose "open command windows here"

you can test a simple tenant using the normal web command ie

dnx web

or you can test 2 tenants at once using

dnx alltenents

the command window will tell you the urls where the web app is listening

open tenant 1 in your browser at
http://localhost:60000 

open tenant 2 in your browser at
http://localhost:60002 

tip: you can see the "alltenants" command in the project.json

Note that these hosts only differ by ports for demo purposes but in production you would use different dns host names per tenant.


Typically there would be a problem logging into 2 different sites using the same hostname "localhost", because the cookies for each tenant would have the same name and therefore logging into one tenant would also log in to another, ie the cookies would step on each other from each tenant. In this demo I used the [saaskit.MultiTenancy](https://github.com/saaskit/saaskit) which enables middleware branching per tenant so we could use different cookie names per tenant in the cookie middleware. You may or may not need different cookie settings per tenant in production, it depends on the specific needs of your project.

The tenants and users are configured in the simpleauthsettings.json file
There are 2 example tenants configured, read that file to find user names and passwords to login to the demo tenants

Notes about tenants and this demo.

this demo is just to show that there is a different tenant with different users and themes and how this can be easily implemented with SimpleAuth

The "HomeController" in this demo does not produce different content per tenant. This is not a cms demo but since you could override any of the home views within a theme folder and since the content is directly in the views you can make different content per tenant that way if you want by using a different theme per tenant and adding different content in the theme specific views.

by default views come from below the normal Views folder

you can override any normal view by copying it up to the corresponding location in the theme folder and then modify it if needed.
you don't have to override any views unless you need to
the Darkly theme has an example overriding the Homes/About.cshtml

typically a custom theme must at elast override the _Layout.cshtml file in order to bring in custom css

only files below wwwroot can be served over https requests, therefore the css cannot be inside the theme folders
so there is also a folder wwwroot/css/themes and each theme links to its own css from the _Layout.cshtml file

Multi tenancy per se is not a feature of SimpleAuth this demo is to show that you can use SimpleAuth in mutli tenant scenarios easily.
It uses Saaskit.MultiTenancy and a few classes implemented directly in the multiTenant.WebApp 
which you could use and modify as part of your own app if you want to.

See the SimpleAuth Readme to understand the limitations of SimpleAuth
https://github.com/joeaudette/cloudscribe.Web.SimpleAuth

even in multi tenant scenarios SimpleAuth is only intended for small numbers of users
I would use for cases if I have a few personal web sites I can run them as tenants if they otherwise use the same software components
ie I could build a content system that is tenant aware and run multiple small sites from a single installation using this approach

