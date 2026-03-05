#2025-05-05
ARG DEV_TAG=10.0-noble@sha256:3af834758c4b1c7bc240e4751e5793d715024bc44cf7366f4bbb176331476db6

FROM mcr.microsoft.com/devcontainers/dotnet:${DEV_TAG}

# [Optional] Uncomment this section to install additional OS packages.
# RUN apt-get update && export DEBIAN_FRONTEND=noninteractive \
#     && apt-get -y install --no-install-recommends <your-package-list-here>

# [Optional] Uncomment this line to install global node packages.
# RUN su vscode -c "source /usr/local/share/nvm/nvm.sh && npm install -g <your-package-here>" 2>&1

# RUN su vscode -c "dotnet tool install --global dotnet-reportgenerator-globaltool"   \
#  && su vscode -c "dotnet tool install --global dotnet-coverage"                     \
#  && su vscode -c "dotnet tool install --global dotnet-ef"                           \
#  && su vscode -c "dotnet dev-certs https --clean"                                   \
#  && su vscode -c "dotnet dev-certs https"