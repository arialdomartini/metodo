This Docker image is the base for the development environment.

It includes:

- an installation of the Linux library `libgit2`
- the needed NuGet packages, copied in `/root/.nuget`

The relative container can be run with the `run.sh` script.

The image requires the NuGet packages to be downloaded: therefore, build it after a NuGet restore.
