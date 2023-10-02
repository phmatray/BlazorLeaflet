using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.Threading.Tasks;
using BlazorLeaflet.Models;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Rectangle = BlazorLeaflet.Models.Rectangle;

namespace BlazorLeaflet;

public static class LeafletInterops
{
    private const string BaseObjectContainer = "window.leafletBlazor";

    private static ConcurrentDictionary<string, (IDisposable, string, Layer)> LayerReferences { get; } = new();

    public static ValueTask Create(IJSRuntime jsRuntime, Map map)
        => jsRuntime.InvokeVoidAsync(
            $"{BaseObjectContainer}.create",
            map, DotNetObjectReference.Create(map));

    private static DotNetObjectReference<T> CreateLayerReference<T>(string mapId, T layer)
        where T : Layer
    {
        var result = DotNetObjectReference.Create(layer);
        LayerReferences.TryAdd(layer.Id, (result, mapId, layer));
        return result;
    }

    private static void DisposeLayerReference(string layerId)
    {
        if (LayerReferences.TryRemove(layerId, out (IDisposable, string, Layer) value))
            value.Item1.Dispose();
    }

    public static ValueTask AddLayer(IJSRuntime jsRuntime, string mapId, Layer layer)
        => layer switch
        {
            TileLayer tileLayer => AddTileLayer(jsRuntime, mapId, tileLayer),
            MbTilesLayer mbTilesLayer => AddMbTilesLayer(jsRuntime, mapId, mbTilesLayer),
            ShapefileLayer shapefileLayer => AddShapefileLayer(jsRuntime, mapId, shapefileLayer),
            Marker marker => AddMarker(jsRuntime, mapId, marker),
            Rectangle rectangle => AddRectangle(jsRuntime, mapId, rectangle),
            Circle circle => AddCircle(jsRuntime, mapId, circle),
            Polygon polygon => AddPolygon(jsRuntime, mapId, polygon),
            Polyline polyline => AddPolyline(jsRuntime, mapId, polyline),
            ImageLayer image => AddImageLayer(jsRuntime, mapId, image),
            GeoJsonDataLayer geo => AddGeoJsonLayer(jsRuntime, mapId, geo),
            _ => throw new NotImplementedException($"The layer {nameof(Layer)} has not been implemented.")
        };

    private static ValueTask AddTileLayer(IJSRuntime jsRuntime, string mapId, TileLayer tileLayer)
        => jsRuntime.InvokeVoidAsync(
            $"{BaseObjectContainer}.addTileLayer",
            mapId, tileLayer, CreateLayerReference(mapId, tileLayer));

    private static ValueTask AddMbTilesLayer(IJSRuntime jsRuntime, string mapId, MbTilesLayer mbTilesLayer)
        => jsRuntime.InvokeVoidAsync(
            $"{BaseObjectContainer}.addMbTilesLayer",
            mapId, mbTilesLayer, CreateLayerReference(mapId, mbTilesLayer));

    public static ValueTask AddShapefileLayer(IJSRuntime jsRuntime, string mapId, ShapefileLayer shapefileLayer)
        => jsRuntime.InvokeVoidAsync(
            $"{BaseObjectContainer}.addShapefileLayer",
            mapId, shapefileLayer, CreateLayerReference(mapId, shapefileLayer));

    public static ValueTask AddMarker(IJSRuntime jsRuntime, string mapId, Marker marker)
        => jsRuntime.InvokeVoidAsync(
            $"{BaseObjectContainer}.addMarker",
            mapId, marker, CreateLayerReference(mapId, marker));

    public static ValueTask AddRectangle(IJSRuntime jsRuntime, string mapId, Rectangle rectangle)
        => jsRuntime.InvokeVoidAsync(
            $"{BaseObjectContainer}.addRectangle",
            mapId, rectangle, CreateLayerReference(mapId, rectangle));

    public static ValueTask AddCircle(IJSRuntime jsRuntime, string mapId, Circle circle)
        => jsRuntime.InvokeVoidAsync(
            $"{BaseObjectContainer}.addCircle",
            mapId, circle, CreateLayerReference(mapId, circle));

    public static ValueTask AddPolygon(IJSRuntime jsRuntime, string mapId, Polygon polygon)
        => jsRuntime.InvokeVoidAsync(
            $"{BaseObjectContainer}.addPolygon",
            mapId, polygon, CreateLayerReference(mapId, polygon));

    public static ValueTask AddPolyline(IJSRuntime jsRuntime, string mapId, Polyline polyline)
        => jsRuntime.InvokeVoidAsync(
            $"{BaseObjectContainer}.addPolyline",
            mapId, polyline, CreateLayerReference(mapId, polyline));

    public static ValueTask AddImageLayer(IJSRuntime jsRuntime, string mapId, ImageLayer image)
        => jsRuntime.InvokeVoidAsync(
            $"{BaseObjectContainer}.addImageLayer",
            mapId, image, CreateLayerReference(mapId, image));

    public static ValueTask AddGeoJsonLayer(IJSRuntime jsRuntime, string mapId, GeoJsonDataLayer geo)
        => jsRuntime.InvokeVoidAsync(
            $"{BaseObjectContainer}.addGeoJsonLayer",
            mapId, geo, CreateLayerReference(mapId, geo));

    public static async ValueTask RemoveLayer(IJSRuntime jsRuntime, string mapId, string layerId)
    {
        await jsRuntime.InvokeVoidAsync($"{BaseObjectContainer}.removeLayer", mapId, layerId);
        DisposeLayerReference(layerId);
    }

    public static ValueTask UpdatePopupContent(IJSRuntime jsRuntime, string mapId, Layer layer)
        => jsRuntime.InvokeVoidAsync(
            $"{BaseObjectContainer}.updatePopupContent",
            mapId, layer.Id, layer.Popup?.Content);

    public static ValueTask UpdateTooltipContent(IJSRuntime jsRuntime, string mapId, Layer layer)
        => jsRuntime.InvokeVoidAsync(
            $"{BaseObjectContainer}.updateTooltipContent",
            mapId, layer.Id, layer.Tooltip?.Content);

    public static ValueTask UpdateShape(IJSRuntime jsRuntime, string mapId, Layer layer)
        => layer switch
        {
            Rectangle rectangle => UpdateRectangle(jsRuntime, mapId, rectangle),
            Circle circle => UpdateCircle(jsRuntime, mapId, circle),
            Polygon polygon => UpdatePolygon(jsRuntime, mapId, polygon),
            Polyline polyline => UpdatePolyline(jsRuntime, mapId, polyline),
            _ => throw new NotImplementedException($"The layer {nameof(Layer)} has not been implemented.")
        };

    public static ValueTask UpdateRectangle(IJSRuntime jsRuntime, string mapId, Rectangle rectangle)
        => jsRuntime.InvokeVoidAsync($"{BaseObjectContainer}.updateRectangle", mapId, rectangle);

    public static ValueTask UpdateCircle(IJSRuntime jsRuntime, string mapId, Circle circle)
        => jsRuntime.InvokeVoidAsync($"{BaseObjectContainer}.updateCircle", mapId, circle);

    public static ValueTask UpdatePolygon(IJSRuntime jsRuntime, string mapId, Polygon polygon)
        => jsRuntime.InvokeVoidAsync($"{BaseObjectContainer}.updatePolygon", mapId, polygon);

    public static ValueTask UpdatePolyline(IJSRuntime jsRuntime, string mapId, Polyline polyline)
        => jsRuntime.InvokeVoidAsync($"{BaseObjectContainer}.updatePolyline", mapId, polyline);

    public static ValueTask FitBounds(IJSRuntime jsRuntime, string mapId, PointF corner1, PointF corner2, PointF? padding, float? maxZoom)
        => jsRuntime.InvokeVoidAsync(
            $"{BaseObjectContainer}.fitBounds",
            mapId, corner1, corner2, padding, maxZoom);

    public static ValueTask PanTo(IJSRuntime jsRuntime, string mapId, PointF position, bool animate, float duration, float easeLinearity, bool noMoveStart)
        => jsRuntime.InvokeVoidAsync(
            $"{BaseObjectContainer}.panTo",
            mapId, position, animate, duration, easeLinearity, noMoveStart);

    public static ValueTask<LatLng> GetCenter(IJSRuntime jsRuntime, string mapId)
        => jsRuntime.InvokeAsync<LatLng>($"{BaseObjectContainer}.getCenter", mapId);

    public static ValueTask<float> GetZoom(IJSRuntime jsRuntime, string mapId)
        => jsRuntime.InvokeAsync<float>($"{BaseObjectContainer}.getZoom", mapId);

        public static async Task SetZoom(IJSRuntime jsRuntime, string mapId, float zoomLevel)
        {
            await jsRuntime.InvokeVoidAsync($"{BaseObjectContainer}.setZoom", mapId, zoomLevel);
        }

        public static ValueTask ZoomIn(IJSRuntime jsRuntime, string mapId, MouseEventArgs e) =>
            jsRuntime.InvokeVoidAsync($"{BaseObjectContainer}.zoomIn", mapId, e);

    public static ValueTask ZoomOut(IJSRuntime jsRuntime, string mapId, MouseEventArgs e)
        => jsRuntime.InvokeVoidAsync($"{BaseObjectContainer}.zoomOut", mapId, e);
}