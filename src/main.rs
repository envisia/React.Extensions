use std::sync::Mutex;
use std::task::Poll;
use std::{future::Future, sync::Arc};

use anyhow::Result;
use envisia_wasm::{ComponentRenderer, TaskResult};

#[derive(Debug, Clone)]
struct EnvisiaPromiseResult {
    data: String,
    error: bool,
}

struct EnvisiaPromise {
    data: Arc<Mutex<Option<EnvisiaPromiseResult>>>,
}

impl Clone for Box<EnvisiaPromise> {
    fn clone(&self) -> Self {
        Box::new(EnvisiaPromise {
            data: self.data.clone(),
        })
    }
}

impl EnvisiaPromise {
    pub fn new() -> Self {
        Self {
            data: Arc::new(Mutex::new(None)),
        }
    }
}

impl Future for EnvisiaPromise {
    type Output = EnvisiaPromiseResult;

    fn poll(
        self: std::pin::Pin<&mut Self>,
        _cx: &mut std::task::Context<'_>,
    ) -> std::task::Poll<Self::Output> {
        match self.data.lock().unwrap().clone() {
            None => Poll::Pending,
            Some(response) => Poll::Ready(EnvisiaPromiseResult {
                data: response.data.clone(),
                error: response.error.clone(),
            }),
        }
    }
}

impl TaskResult for EnvisiaPromise {
    fn success(&self, result: String) {
        let mut inner = self.data.lock().unwrap();
        *inner = Some(EnvisiaPromiseResult {
            data: result,
            error: false,
        });
    }

    fn error(&self, result: String) {
        let mut inner = self.data.lock().unwrap();
        *inner = Some(EnvisiaPromiseResult {
            data: result,
            error: false,
        });
    }
}

#[async_std::main]
async fn main() -> Result<()> {
    let renderer = ComponentRenderer::new();

    let callback = Box::new(EnvisiaPromise::new());

    renderer.render_component(
        callback.clone(),
        "/Users/schmitch/wasm/envisia.component.wasm".to_string(),
        "SoftwareLinks".to_string(),
        "{\"httpsEnabled\": false}".to_string(),
    );

    let awaited_callback = callback.await;
    println!("Values: {:?}", awaited_callback);

    Ok(())
}
