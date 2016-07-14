## Introduction

Do you like "archiving" data? Especially ones that tends to be ordered sequentially numerically? And does those data tend to be images of a sort? Fret no more!

![The program upon starting](/Screenshots/main_window.png)![The advanced batch manager window](/Screenshots/adv_batch_mngr.png)

## Usage

First, you need to get an URL that the program can work off of. Currently, the program is designed to work with three different types of URL that are very similar. They are:

- MangaHere (example: http://www.mangahere.co/manga/koko_ni_iru_yo/v01/c001/43.html)
- MangaFox (example: http://mangafox.me/manga/koko_ni_iru_yo/v01/c001/3.html)
- xkcd (example: https://xkcd.com/78/)

The above URLS can be obtained by copying it from the address bar of a page of the manga.

After getting the URL, feed it into the "Main URL" field, then provide the desired starting and ending page numbers. This can be, for example, "2" and "43" respectively, if using the above examples (excluding xkcd), but can also be any number in between, so long as the ending page number is always higher than the starting page number.

 The starting page number always has to be filled no matter which mode you are using the program in, but the ending page number is only needed for the Batch modes.

 The Preview modes, as of now, just streams the images, whereas the Download modes will stream, and then subsequently save the image streams to a designated path. In the future, this may very well change.
 
## "Changelog"

- 12 July 2016: Initial build version of 0.0.1.0. Able to parse and stream/download images (sequentially) from two different manga sites, albeit only part of the wide selection available due to parsing issues.
- 13 July 2016: Reworked methods to enable wider compatability with the available selection of manga from the two sites, at the cost of processing speed.
- 13 July 2016: Added xkcd support.
- 14 July 2016: Added an advanced option to downloading images in batches. Now you can queue up "jobs", and leave it running while you go do something else!
- 14 July 2016: Migrated most of the downloading code to a new class.
- 15 July 2016: Resolved a very minor bug that caused part of the program to simply fail. Adapted @WhiteXZ's [Downloader.cs](https://gist.github.com/WhiteXZ/1e5c19ccf3f69e68744de21a805f3bf4) into the program.
- 15 July 2016: Split into new branch, in light of a potential rework.
 
## Installation

This was coded in C#, .NET Framework 4, with Visual Studio 2010. This repository contains the complete files that constitutes a working version of the program.

![.NET Framework 4](https://public-dm2306.files.1drv.com/y3pXtgOa3VAq1KJC17mOmtDEPHusKHAB9-7yuC54hI8Y09iMHkj7cSTqPzm-c2hu7OPOEI-ixow1bGvhOElUZRiFtFmgt8BNExvufrWkuXzyzmYY1WE-v_-1nYVuGdbqrPq/NET-Frmwrk_h_rgb.png?rdrts=142979546)

Lack .NET Framework 4 (Not the client profile)? Download it [here](https://www.microsoft.com/en-us/download/details.aspx?id=17718)!

## License

This project is licensed under the [GNU General Public License v3](LICENSE.md).