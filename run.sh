docker run \
       -v $(pwd)/:/app \
       -v /:/volume \
       --name metodo \
       --rm \
       -ti \
       --workdir /app \
       libgit2 bash

