# export DYLD_LIBRARY_PATH="/Users/schmitch/projects/envisia/tooling/Envisia.React.Extensions/target/debug/:${DYLD_LIBRARY_PATH:-}"
# cargo install uniffi-bindgen-cs --git https://github.com/NordSecurity/uniffi-bindgen-cs --tag v0.2.3+v0.23.0
cargo build
uniffi-bindgen-cs src/envisia.udl

cd Envisia.Wasm.Extensions
# dotnet clean && dotnet run
cd ..
