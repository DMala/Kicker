# Kicker
A GUI frontend for Visual Pinball

Provides a more attractive way to view and launch your tables in [Visual Pinball](https://github.com/vpinball/vpinball).

* Simple configuration, just give it a path to your Visual Pinball installation and the folder containing your tables.  It will scan and display a grid of your tables with the best information it can find - embedded in the VPX file, in the filename, or some combination of the two.
* Click 'Start', double-click the cell, or press ENTER with a selected cell to launch the table
* Optionally supports launching Joy2Key prior to opening the table
* 'Random' button allow for launching a randomly selected table

## Todo
* Explicit support for Visual Pinball versions other than X.  Should generally work but not tested yet.
* Improve initial startup/configuration experience
* Better UI when scanning tables
* Controller support for navigating the UI
* Support for multiple Visual Pinball versions with tables grouped in the UI
* Support for multiple table locations
* Automatic re-scan when the contents of table dirs change
* Detailed info view for tables
* Official release

## Build
Builds with Microsoft Visual Studio 2022 Community Edition.
