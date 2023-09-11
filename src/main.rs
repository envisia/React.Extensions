use std::io::Read;
use std::{
    fs::{self, File},
    time::Instant,
};

use anyhow::Result;
use wasmtime::{
    component::{Component, Linker},
    Config, Engine, PoolingAllocationConfig, Store, WasmBacktraceDetails,
};
use wasmtime_wasi::preview2::{command, Table, WasiCtx, WasiCtxBuilder, WasiView};

use std::io::Write;

wasmtime::component::bindgen!({
    world: "envisia",
    path: "envisia-finder.wit",
    async: true,
});

fn get_file_as_byte_vec(filename: &String) -> Vec<u8> {
    let mut f = File::open(&filename).expect("no file found");
    let metadata = fs::metadata(&filename).expect("unable to read metadata");
    let mut buffer = vec![0; metadata.len() as usize];
    f.read(&mut buffer).expect("buffer overflow");

    buffer
}

#[async_std::main]
async fn main() -> Result<()> {
    let mut now = Instant::now();
    let builder = WasiCtxBuilder::new().inherit_stdio();
    let mut table = Table::new();
    let wasi = builder.build(&mut table)?;

    let mut config = Config::new();
    config.cache_config_load_default().unwrap();
    config.wasm_backtrace_details(WasmBacktraceDetails::Enable);
    config.wasm_component_model(true);
    config.async_support(true);

    println!("took1: {:.2?}", now.elapsed());
    now = Instant::now();

    let engine = Engine::new(&config)?;
    let mut linker = Linker::new(&engine);
    println!("took2: {:.2?}", now.elapsed());
    now = Instant::now();


    let wasm_file = "/Users/schmitch/projects/envisia/finder/finder-v2/Finder.Web/ClientApp/build/ssr/envisia.component.wasm";

    let precompiled = engine.precompile_component(&get_file_as_byte_vec(&wasm_file.to_string()))?;
    println!("took3: {:.2?}", now.elapsed());
    now = Instant::now();

    let component = unsafe { Component::deserialize(&engine, precompiled)? }; // Component::from_file(&engine, wasm_file).unwrap();

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

    command::add_to_linker(&mut linker).unwrap();
    let mut store = Store::new(&engine, CommandCtx { table, wasi });

    let (instance, _instance) = Envisia::instantiate_async(&mut store, &component, &linker).await?;

    println!("took9: {:.2?}", now.elapsed());
    now = Instant::now();
    let res = instance
        .call_render_component(&mut store, "SoftwareLinks", "{\"httpsEnabled\": false}")
        .await?;
    

    let mut stdout = std::io::stdout();
    let mut lock = stdout.lock();
    writeln!(lock, "{}", res)?;

    println!("took10: {:.2?}", now.elapsed());
    Ok(())
}
