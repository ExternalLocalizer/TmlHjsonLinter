FROM mcr.microsoft.com/dotnet/runtime:8.0

RUN apt-get update && apt-get install -y \
    curl \
    jq \
    && apt-get clean \
    && rm -rf /var/lib/apt/lists/*

RUN curl -s https://api.github.com/repos/ExternalLocalizer/TmlHjsonLinter/releases/latest \
    | jq -r '.assets[] | select(.name == "TmlHjsonLinter") | .browser_download_url' \
    | xargs -n 1 curl -L -o /usr/local/bin/TmlHjsonLinter \
    && chmod +x /usr/local/bin/TmlHjsonLinter

ENTRYPOINT ["TmlHjsonLinter"]
