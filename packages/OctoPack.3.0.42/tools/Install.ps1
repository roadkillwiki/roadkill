param($installPath, $toolsPath, $package, $project)
    # This is the MSBuild targets file to add
    $targetsFile = [System.IO.Path]::Combine($toolsPath, $package.Id + '.targets')
 
    # Need to load MSBuild assembly if it's not loaded yet.
    Add-Type -AssemblyName 'Microsoft.Build, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'

    # Grab the loaded MSBuild project for the project
    $msbuild = [Microsoft.Build.Evaluation.ProjectCollection]::GlobalProjectCollection.GetLoadedProjects($project.FullName) | Select-Object -First 1
 
    # Make the path to the targets file relative.
    $projectUri = new-object Uri($project.FullName, [System.UriKind]::Absolute)
    $targetUri = new-object Uri($targetsFile, [System.UriKind]::Absolute)
    $relativePath = [System.Uri]::UnescapeDataString($projectUri.MakeRelativeUri($targetUri).ToString()).Replace([System.IO.Path]::AltDirectorySeparatorChar, [System.IO.Path]::DirectorySeparatorChar)
 
    # Add the import with a condition, to allow the project to load without the targets present.
    $import = $msbuild.Xml.AddImport($relativePath)
    $import.Condition = "Exists('$relativePath')"

    # Add a target to fail the build when our targets are not imported
    $target = $msbuild.Xml.AddTarget("EnsureOctoPackImported")
    $target.BeforeTargets = "BeforeBuild"
    $target.Condition = "'`$(OctoPackImported)' == ''"

    # if the targets don't exist at the time the target runs, package restore didn't run
    $errorTask = $target.AddTask("Error")
    $errorTask.Condition = "!Exists('$relativePath') And ('`$(RunOctoPack)' != '' And `$(RunOctoPack))"
    $errorTask.SetParameter("Text", "You are trying to build with OctoPack, but the NuGet targets file that OctoPack depends on is not available on this computer. This is probably because the OctoPack package has not been committed to source control, or NuGet Package Restore is not enabled. Please enable NuGet Package Restore to download them. For more information, see http://go.microsoft.com/fwlink/?LinkID=317567.");
    $errorTask.SetParameter("HelpKeyword", "BCLBUILD2001");

    # if the targets exist at the time the target runs, package restore ran but the build didn't import the targets.
    $errorTask = $target.AddTask("Error")
    $errorTask.Condition = "Exists('$relativePath') And ('`$(RunOctoPack)' != '' And `$(RunOctoPack))"
    $errorTask.SetParameter("Text", "OctoPack cannot be run because NuGet packages were restored prior to the build running, and the targets file was unavailable when the build started. Please build the project again to include these packages in the build. You may also need to make sure that your build server does not delete packages prior to each build. For more information, see http://go.microsoft.com/fwlink/?LinkID=317568.");
    $errorTask.SetParameter("HelpKeyword", "BCLBUILD2002");

    $project.Save()