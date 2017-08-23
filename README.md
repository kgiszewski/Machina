# Machina

An Umbraco Chauffeur extension to run one-time utilities (or as needed) on your Umbraco sites.

## Property Editor Migrations

Migrate from Property Editors from the old non-UDI to the new UDI format.

**PLEASE BACKUP YOUR DATABASE FIRST**

To use you will need to do the following:

1) Install [Chauffeur.Runner](https://www.nuget.org/packages/Chauffeur.Runner/0.9.0) (recommended to use v0.9.0) to your web project. This will allow you to execute these scripts from the command line.
2) Make sure the `Machina.dll` is in you web `/bin` folder. Get it on [Nuget](https://www.nuget.org/packages/Umbraco.Machina).
3) Start the runner by executing `~/bin/Chauffeur.Runner.exe`
4) Type `help` and press `ENTER`. This will show you the commands you can run.

**You should read the rest of this before proceeding (including FAQ's)!**

![help](assets/machina.png?v=0.2.0)

To run a command, type the command name and press `ENTER`. For instance to run the `migrate content picker` command, type `machina-mcp` and press `ENTER`.

This will provide you preview output against your database. At this time you see if the proposed changes seem legit.

You'll likely get no output because you probably haven't changed your `ContentPicker` to `ContentPicker2`.

Log into Umbraco, change any datatype using `Umbraco.ContentPicker` to `Umbraco.ContentPicker2` and save. Any prevalues won't move over so you'll wanna update those too.

Now re-run the preview (i.e. `machina-mcp <ENTER>`).

If you're happy with the results and want to alter the DB, type `machina-mcp -p:1<ENTER>`.

At this point your site will be broken if you visit it. It's because the underlying content has been updated in the DB but the frontend is operating off the cache values.

The cached values are used in the `Umbraco.ContentPicker2` property value converter and will throw an exception.

To fix publish all of your nodes. You can do so by right-clicking the root level items (one at a time) and selecting 'Publish'. This isn't the same as 'Republish Entire Site' which is available at the top.

**BACKUP YOUR DB IN CASE IT GOES WRONG. PRACTICE LOCALLY BEFORE ATTEMPTING ON PROD!**

## Commands (v0.2.0+)

### Migrate Nested Content
`machina-mnc`

Requires `-ncdtpa`, and `-udi`
Optionally `-f`, `-p`

Migrates Nested Content values to use UDI.

### Migrate Content Picker
`machina-mcp `

Optionally `-f`, `-p`

Migrates Content Picker.

### Migrate Media Picker
`machina-mmp`

Optionally `-f`, `-p`

Migrates Media Picker.

### Migrate MNTP
`machina-mmntp`

Optionally `-f`, `-p`

Migrates MNTP values to UDI. At this time only supports `content` and not `media` or `member`. If you need to do this, ya might wanna fork this repo and have a go at it.

## Args
`-udi:<media|content>` - Sets whether to use Media or Content for UDI generation.

`-f:<docTypeAlias>` - Filters the content based on a doctype alias.

`-p:1` - Instructs Machina to save the results to the DB **BACKUP YOUR DB FIRST**

`-ncdtpa:<docTypePropertyAlias>` - Which JSON field are we doing crazy REGEX on?

## FAQ

**Where do I get the `Machina.dll`?**
Right now can clone this repo and build it or off [Nuget](https://www.nuget.org/packages/Umbraco.Machina).

**What about 'xyz' property type?**
There are a few other property types not covered in the migration scripts. Those are `Archetype`, `Folder Browser`, `Member Picker` and `Related Links`. Send me a PR.

**Can I just test a small set first?**
You can pass a doctype to the migrations in this form `machina-mcp -f:homepage` to limit the content to the `homepage` doctype. Use `machina-mcp -p:1 -f:homepage` to persist the changes to just that doctype.

**I don't see the commands I'm expecting.**
Confirm the `Machina.dll` is in your `/bin`. Visual Studio doesn't like to copy DLL's (even if they are referenced) to the /bin unless there is a usage somewhere in code. You can force it by using a dummy class like this:
```
public class MachinaBootstrapper
{
    private static void Dummy()
    {
        Action<Type> noop = _ => { };
        var dummy = typeof(Machina.Migrations.MigrationHelper);
        noop(dummy);
    }
}
```

Or you can copy it directly from the Nuget package folder and drop it into the `/bin`.

This is an annoying 'feature' that can be read about [here](https://stackoverflow.com/questions/15816769/dependent-dll-is-not-getting-copied-to-the-build-output-folder-in-visual-studio).

## Thanks
Thanks to [Aaron Powell](https://github.com/aaronpowell) (Chauffeur author) and Tom Fulton for letting me share :)
