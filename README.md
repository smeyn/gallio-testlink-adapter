# gallio-testlink-adapter

The Testlink Adapter Collection
These pages started as an adapter that connected Gallio to TestLink. It has grown now and we have:

A C# library wrapping the TestLink Api
A Gallio Adapter adapter that exports Gallio Results to TestLink
A NUnit Adapter that exports NUnit Results to TestLink ## Adapter Does not Work? ## Check the version of MBUnit or NUnit respectively you are using. With MBUnit the project needs to be recompiled whenever a new version comes out. With NUnit that may be the same case (not yet verified).
Update

1 Jan 2013: new version of Binaries for NUnit This version has been recompiled to work with NUnit v 2.6.2

May 2012 :

new version of the testlink API V1.3 to fix defects and incompatibilities with V 1.9 of Testlink.
new Version for Gallio Adapter V1.2
new version for NUnit aAdapter V.12 This is a major rework, due to the fact that TestLink's API has changed considerably and been extended. Most of this API has been wrappedin .NET (with the excpetion of Requiremetns handling.
Also for the Gallio and Nunit adapters, a new parameter for the TestlinkFixture Attribute has been added: ConfigFile takes a path to an XML config file. This config file allows provision of default values for all parameters in the TestlinkFixture attribute. This way changes can be made such as pointing to a different Testlink host without having to recompile the test code.

25 Nov 2009: New binaries and source for NUnit 2.5.2

6 May 2009: new binaries are up for Gallio V 3.0.6
