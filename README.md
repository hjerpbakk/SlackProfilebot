# Profilebot - the profile checking Slackbot for you!

[![Build status](https://ci.appveyor.com/api/projects/status/498ui2cq6tpxg7c7/branch/master?svg=true)](https://ci.appveyor.com/project/Sankra/slackprofilebot/branch/master) [![codecov](https://codecov.io/gh/Sankra/SlackProfilebot/branch/master/graph/badge.svg)](https://codecov.io/gh/Sankra/SlackProfilebot)

In a large Slack team, it's important that user profiles are completed enough for the users to recognise each other. Profilebot is a simple bot used for validating Slack user profiles. It is written in C# and it's easy to extend for all your profile checking needs. It runs out of the box either as a console app (Windows) or a Windows service.

<p align="center">
<img src="https://raw.githubusercontent.com/Sankra/SlackProfilebot/master/logo.png" alt="Profilebot" width="256" />
</p>

## Usage in Slack

Profilebot runs like any other Slackbot. Open a direct message to Profilebot and start talking.

### As admin

If you are the admin of Profilebot, the following commands are available for you:

- `validate all users`
- `notify all users`
- `validate @user`
- `notify @user`
- `whitelist @user`

Validating will inform you of the user's profile status, while notify will also send a direct message to the specific user(s).

<p align="center">
<img src="https://raw.githubusercontent.com/Sankra/SlackProfilebot/master/img/AdminUsage.png" alt="Admin usage" width="834" />
</p>

### As a regular user

As a regular user, anything you say will have the same effect: Profilebot will look at your profile and tell you if any information is missing.

## Download & setup

1. Clone the repository: `git clone https://github.com/Sankra/SlackProfilebot.git`

2. Go to `Hjerpbakk.Profilebot.Runner/Configuration` and create a copy of `config.default.json`. 

3. Rename the copy to `config.json`.

4. Go to your Team's Slack [bot integration page](https://my.slack.com/services/new/bot).

5. Enter 'profilebot' as the Username and click the 'Add Bot Integration' button.

6. Copy the generated API token and paste it as your  `apiToken` in `config.json`:

   ```
   {
   	"apiToken": "xxxx-00000000000-xxxxxxxxxxxxxxxxxxxxxxxx",
   	"adminUserId": ""  
   }
   ```

7. One user must be configured to be Profilebots admin user. This need not be an actual Slack admin, but the id must be a valid user id in the Slack team. Run https://api.slack.com/methods/users.list/test to find the user id you want to use.

8. Copy the user id and paste it as your `adminUserId` in `config.json`:

   ```
   {
   	"apiToken": "xxxx-00000000000-xxxxxxxxxxxxxxxxxxxxxxxx",
   	"adminUserId": "x0xxx0000"  
   }
   ```

9. In Azure, create a new Face API resource
    1. From the Azure Portal, select "Create a resource"
    2. In the sidebar, select `AI + Machine Learning`, then `Face` to create a Face API resource
    3. Give it a fitting name and finish the form. The resource may take a while to complete

10. Add the Face API configuration to the ProfileBot solution
    1.  On the sidebar of the Face API resource, select `Quick start`
    2.  Copy the contents of `Key1` and `Endpoint` into `faceAPIAccessKey` and `faceAPIURL` respectively in `config.json`
    3.  At the end of the Face API URL, add `/face/v1.0`

11. In Azure, create a new Blob Storage, or use an existing one
    1.  If you don't have an existing one, select `Create a resource` from the portal
    2.  Select `Storage` from the sidebar, then `Storage Account`
    3.  Give it a fitting name and set `Account kind` to `BlobStorage`
    4.  Complete the form. The resource may take a while to complete

12. Add the Blob Storage configuration to the ProfileBot solution
    1.  In the sidebar of the Storage Account, select `Access keys`
    2.  Copy the contents of `Storage account name` and `Key` into `blobStorageAccountName` and `blobStorageAccessKey` respectively in `config.json`
    3.  The application will during its first run create a new container in the Blob Storage, titled `whitelist`

13. Build and run `Hjerpbakk.Profilebot.sln`.

You should now be able to send direct messages to Profilebot in your team's Slack. 

## Customize the profile validation

At my company, [DIPS AS](https://dips.com) we use the following validation rules:

- The email used must be a DIPS AS email
- Username must be the same as in all other internal systems
- Both first name and last name must be given
- *What I do* must tell others what you do
- A profile image must be set and be a picture of a human face

Customising the profile validation is easy, and you have two choices for how to do this.

### 1. Extend SlackProfileValidator

Open `Hjerpbakk.ProfileBot.SlackProfileValidator`  and edit the `ProfileValidationResult ValidateProfile(SlackUser user)` method.

### 2. Inherit from ISlackProfileValidator and create your own validator

1. Create a new class and inherit from `ISlackProfileValidator`.
2. In `Hjerpbakk.ProfileBot.Runner.ProfileBotHost`, edit the `IServiceContainer CompositionRoot(string slackToken, string adminUserId)` method. Instead of `serviceContainer.Register<ISlackProfileValidator, SlackProfileValidator>();`, register your own validation class.

<p align="center">
<img src="https://raw.githubusercontent.com/Sankra/SlackProfilebot/master/img/dependency-graph.png" alt="Profilebot has a simple dependency graph" width="605" />
</p>

## Running

Thanks to [Topshelf](http://topshelf-project.com), running Profilebot either as a service or a console app is super easy.

To run as a console app, just run `Hjerpbakk.Profilebot.Runner.exe`. To exit, press `CTRL + C`.

To install as a service, simply run the following in `command prompt`:

```
Hjerpbakk.Profilebot.Runner.exe install
Hjerpbakk.Profilebot.Runner.exe start
```
