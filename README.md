export DYLD_LIBRARY_PATH="./target/debug/:${DYLD_LIBRARY_PATH:-}"
# cargo install uniffi-bindgen-cs --git https://github.com/NordSecurity/uniffi-bindgen-cs --tag v0.2.3+v0.23.0

uniffi-bindgen-cs src/envisia.udl
https://git.envisia.io/finder/finder/-/merge_requests/3328/diffs && cd Finder.Web/ClientApp && npm install && npm run wasm