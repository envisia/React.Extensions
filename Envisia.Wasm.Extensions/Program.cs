Console.WriteLine("Start!");

var renderer = new ComponentRenderer();

renderer.RenderComponent(
    new SimpleCallback(),
     "/Users/schmitch/projects/envisia/finder/finder-v2/Finder.Web/ClientApp/build/ssr/envisia.component.wasm",
      "SoftwareLinks",
       "{\"httpsEnabled\": false}");

await Task.Delay(TimeSpan.FromMinutes(10));