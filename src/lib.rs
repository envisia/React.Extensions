use std::fmt::Debug;
use std::time::Instant;
use wasmtime::{
    component::{Component, Linker},
    Config, Engine, Store, WasmBacktraceDetails,
};
use wasmtime_wasi::preview2::{Table, WasiCtx, WasiCtxBuilder, WasiView, command};
use async_std::task;

wasmtime::component::bindgen!({
    world: "envisia",
    path: "envisia-finder.wit",
    async: true,
});

pub trait TaskResult: Send + Sync + Debug {
    fn success(&self, result: String);
    fn error(&self, result: String);
}

struct ComponentRenderer {}

impl ComponentRenderer {
    pub fn new() -> Self {
        Self {}
    }

    fn render_component(
        &self,
        callback: Box<dyn TaskResult>,
        file: String,
        name: String,
        values: String,
    ) {
        callback.success(format!("file: {}, name: {}, values: {}", file, name, values).to_string());

        let _ = task::spawn(async move {
            let mut now = Instant::now();
            let builder = WasiCtxBuilder::new().inherit_stdio();
            let mut table = Table::new();
            let wasi = builder.build(&mut table).unwrap();

            let mut config = Config::new();
            config.cache_config_load_default().unwrap();
            config.wasm_backtrace_details(WasmBacktraceDetails::Enable);
            config.wasm_component_model(true);
            config.async_support(true);
            callback.success("0".to_string());

            let engine = Engine::new(&config).unwrap();
            let mut linker = Linker::new(&engine);

            let component = Component::from_file(&engine, file).unwrap();

            struct CommandCtx {
                table: Table,
                wasi: WasiCtx,
            }
            impl WasiView for CommandCtx {
                fn table(&self) -> &Table {
                    &self.table
                }
                fn table_mut(&mut self) -> &mut Table {
                    &mut self.table
                }
                fn ctx(&self) -> &WasiCtx {
                    &self.wasi
                }
                fn ctx_mut(&mut self) -> &mut WasiCtx {
                    &mut self.wasi
                }
            }
            callback.success("1".to_string());
            let elapsed = now.elapsed();
            now = Instant::now();
            callback.success(format!("1.5: {:.2?}", elapsed).to_string());

            let mut store = Store::new(&engine, CommandCtx { table, wasi });


            let elapsed = now.elapsed();
            now = Instant::now();
            callback.success(format!("2: {:.2?}", elapsed).to_string());
            let (instance, _instance) = Envisia::instantiate_async(&mut store, &component, &linker)
                .await
                .unwrap();

            let elapsed = now.elapsed();
            now = Instant::now();

            command::add_to_linker(&mut linker).unwrap();
            callback.success(format!("3: {:.2?}", elapsed).to_string());
            let res = instance
                .call_render_component(&mut store, name.as_str(), values.as_str())
                .await
                .unwrap();

                callback.success(format!("Length: {}", res.len()));
                callback.success(res);
                /*
            let elapsed2 = now.elapsed();
            match res {
                Ok(s) => callback.success(format!("4: {} | {:.2?}", s, elapsed2).to_string()),
                Err(err) => callback.error(err.to_string())
            } */
        });

        /*

        Ok(res) */
    }
}

/*
https://github.com/NordSecurity/uniffi-bindgen-cs
#[derive(Debug, uniffi::Error)]
pub enum EnvisiaError {
    General { error_code: String },
}

impl From<wasmtime::Error> for EnvisiaError {
    fn from(value: wasmtime::Error) -> Self {
        EnvisiaError::General {
            error_code: format!("error: {}", value),
        }
    }
}
*/
// uniffi::setup_scaffolding!();
uniffi::include_scaffolding!("envisia");
// ffi_envisia_a9e8_TaskResult_init_callback
// ffi_envisia_e1ae_TaskResult_init_callback
