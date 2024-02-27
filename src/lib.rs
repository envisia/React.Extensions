use async_std::task;
use std::fmt::Debug;
use wasmtime::{
    component::{Component, Linker},
    Config, Engine, Store, WasmBacktraceDetails,
};
use wasmtime_wasi::preview2::{command, Table, WasiCtx, WasiCtxBuilder, WasiView};

wasmtime::component::bindgen!({
    world: "envisia",
    path: "envisia-finder.wit",
    async: true,
});

pub trait TaskResult: Send + Sync {
    fn success(&self, result: String);
    fn error(&self, result: String);
}

pub struct ComponentRenderer<'a> {
    _instance_id: i32,
    engine: &'a Engine,
}

impl<'a> ComponentRenderer<'a> {
    pub fn new(file: String) -> Self {
        let builder = WasiCtxBuilder::new().inherit_stdio();
        let mut table = Table::new();
        let wasi = builder.build(&mut table).unwrap();

        let mut config = Config::new();
        config.cache_config_load_default().unwrap();
        config.wasm_backtrace_details(WasmBacktraceDetails::Enable);
        config.wasm_component_model(true);
        config.async_support(true);

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

        let mut store = Store::new(&engine, CommandCtx { table, wasi });

        Self { _instance_id: 1 }
    }

    pub fn render_component(
        &self,
        callback: Box<dyn TaskResult>,
        file: String,
        name: String,
        values: String,
    ) {
        let _ = task::spawn(async move {
            let (instance, _instance) = Envisia::instantiate_async(&mut store, &component, &linker)
                .await
                .unwrap();

            command::add_to_linker(&mut linker).unwrap();
            let res = instance
                .call_render_component(&mut store, name.as_str(), values.as_str())
                .await
                .unwrap();

            println!("success: {}", res);

            callback.success(res);
        });
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
