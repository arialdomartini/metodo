using LibGit2Sharp;

namespace Metodo.CommandLine
{
    struct Author
    {
        internal string Name { get; private set; }
        internal string Email { get; private set; }

        internal static Author From(Signature signature) =>
            new Author {Name = signature.Name, Email = signature.Email};

    }
}