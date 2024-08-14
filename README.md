![sentakki](assets/logov3.png)

---
[![.NET Core](https://github.com/LumpBloom7/sentakki/workflows/.NET%20Core/badge.svg)](https://github.com/LumpBloom7/sentakki/actions?query=workflow%3A%22.NET+Core%22)
[![CodeFactor](https://www.codefactor.io/repository/github/lumpbloom7/sentakki/badge)](https://www.codefactor.io/repository/github/lumpbloom7/sentakki)
[![Crowdin](https://badges.crowdin.net/sentakki/localized.svg)](https://crowdin.com/project/sentakki)
[![Discord Shield](https://discordapp.com/api/guilds/700619421466624050/widget.png?style=shield)](https://discord.gg/CQPNADu)

[![ko-fi](https://www.ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/E1E01N56M)

sentakki is a recreation of maimaiDX for use within osu, taking advantage of the longevity and the osu ecosystem for features such as multiplayer, beatmap listing, and leaderboards.

## Status


All the essential note types have been implemented. Features like editor is being developed on the side, since lazer doesn't support third party ruleset editors yet. PP/diff calc haven't been given considered yet, and it is currently open to ideas.

## Demo

https://github.com/user-attachments/assets/5d8723ee-98c1-48af-92f5-5b3c8a633e9f

## Trying the ruleset

Prebuilt binaries are provided for users who doesn't want to create a development environment. Releases are guaranteed to work with the latest version of lazer at the time of release.

| [Releases](https://github.com/lumpbloom7/sentakki/releases/) |
| ------------------------------------------------------------ |

| [Installation Guide](https://github.com/LumpBloom7/sentakki/wiki/Ruleset-installation-guide) |
| -------------------------------------------------------------------------------------------- |

| [Sentakki wiki](https://github.com/LumpBloom7/sentakki/wiki/) |
| ------------------------------------------------------------- |

## Debugging and Developing

Some prerequisites are required before attempting to debug or develop:

* A desktop platform with the .NET 8 SDK or higher installed.
* An IDE with support for C#, providing auto completion and syntax highlighting. I recommend using Visual Studio 2019 or Visual Studio Code.
* Other requirements are shared with osu!lazer and osu!framework

### Downloading the source code

Clone the repository:

```sh
git clone https://github.com/lumpbloom7/sentakki
cd sentakki
```

To update the source code to the latest commit, run the following command inside the osu directory:

```sh
git pull
```

### Building

Using the `Build` command from your IDE should generate a DLL file within the output directory. If you're debugging or developing, it is a bit more convenient to run the `VisualTests` project instead since that'll remove the need to copy the dll to your lazer directory.

You can also build sentakki from the command-line with a single command:

```sh
dotnet build osu.Game.Rulesets.Sentakki
```

## Contributing

There are a few ways one can look to contribute to sentakki.

### Code contributions

If you are interested in implementing new features or improving current features, you can fork the repository and develop the feature/improvement on a topic branch of your fork before PR'ing the changes to this repository.

### Localization contributions

Want to see Sentakki display text in your language? You can help localize Sentakki via the [project's crowdin page](https://crowdin.com/project/sentakki).

### Feedback / bug reports

Notice a problem during gameplay? Feel free to leave an issue/suggestion over at [GitHub issues](https://github.com/LumpBloom7/sentakki/issues).

## Licence

Sentakki is licensed under the [MIT licence](https://opensource.org/licenses/MIT). Please see the licence file for more information. tl;dr you can do whatever you want as long as you include the original copyright and license notice in any copy of the software/source.

Do take note that project dependencies may not share the same license.
