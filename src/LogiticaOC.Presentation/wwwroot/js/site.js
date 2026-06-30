// site.js — LogiticaOC

document.addEventListener('DOMContentLoaded', () => {

    // ── Fecha + hora en vivo ──────────────────────────────────────────
    const datetimeEl = document.getElementById('liveDatetime');
    if (datetimeEl) {
        const dias = ['domingo','lunes','martes','miércoles','jueves','viernes','sábado'];
        const meses = ['enero','febrero','marzo','abril','mayo','junio','julio','agosto','septiembre','octubre','noviembre','diciembre'];

        function actualizarDatetime() {
            const now = new Date();
            const dia = dias[now.getDay()];
            const fecha = `${dia.charAt(0).toUpperCase() + dia.slice(1)}, ${now.getDate()} de ${meses[now.getMonth()]} ${now.getFullYear()}`;
            const hora = now.toLocaleTimeString('es-ES', { hour: '2-digit', minute: '2-digit', second: '2-digit' });
            datetimeEl.textContent = `${fecha} · ${hora}`;
        }
        actualizarDatetime();
        setInterval(actualizarDatetime, 1000);
    }

    // ── Toggle dark / light mode ──────────────────────────────────────
    const html       = document.documentElement;
    const toggleBtn  = document.getElementById('themeToggle');
    const themeIcon  = document.getElementById('themeIcon');
    const themeLabel = document.getElementById('themeLabel');

    // Recuperar preferencia guardada
    const savedTheme = localStorage.getItem('oc-theme') || 'dark';
    applyTheme(savedTheme);

    if (toggleBtn) {
        toggleBtn.addEventListener('click', () => {
            const next = html.getAttribute('data-theme') === 'dark' ? 'light' : 'dark';
            applyTheme(next);
            localStorage.setItem('oc-theme', next);
        });
    }

    function applyTheme(theme) {
        html.setAttribute('data-theme', theme);
        if (!themeIcon || !themeLabel) return;
        if (theme === 'light') {
            themeIcon.className  = 'bi bi-moon-fill';
            themeLabel.textContent = 'Modo oscuro';
        } else {
            themeIcon.className  = 'bi bi-sun-fill';
            themeLabel.textContent = 'Modo claro';
        }
    }

    // ── Auto-dismiss alerts ───────────────────────────────────────────
    document.querySelectorAll('.alert.alert-success, .alert.alert-danger').forEach(el => {
        setTimeout(() => {
            if (typeof bootstrap !== 'undefined' && bootstrap.Alert) {
                bootstrap.Alert.getOrCreateInstance(el).close();
            }
        }, 4000);
    });

    // ── Select2 estático ──────────────────────────────────────────────
    if (typeof jQuery !== 'undefined' && typeof jQuery.fn.select2 !== 'undefined') {
        jQuery('.select2-simple').select2({
            placeholder: 'Seleccionar...',
            allowClear: true,
            width: '100%'
        });
    }
});

// ── LÓGICA AJAX, SPA, DRAFTS & MODALES (JQUERY) ───────────────────────
if (typeof jQuery !== 'undefined') {
    jQuery(function ($) {

        var isModalDraft = false;

        // Helper para reinicializar plugins en nuevo contenido
        function reinicializarPlugins() {
            if (typeof $.fn.select2 !== 'undefined') {
                $('.select2-simple').select2({
                    placeholder: 'Seleccionar...',
                    allowClear: true,
                    width: '100%'
                });
            }
        }

        // Función modular para cargar páginas de forma SPA
        function cargarPaginaSPA(url, pushState) {
            var container = $('.content-area');
            container.css('opacity', '0.4'); // feedback visual

            $.ajax({
                url: url,
                type: 'GET',
                success: function (html) {
                    // Parsear el HTML completo para extraer solo el contenedor principal
                    var parser = new DOMParser();
                    var doc = parser.parseFromString(html, 'text/html');
                    
                    var newContentEl = doc.querySelector('.content-area');
                    var newContent = newContentEl ? newContentEl.innerHTML : html;
                    
                    container.html(newContent).css('opacity', '1');

                    // Actualizar active en el sidebar
                    $('.sidebar .nav-link').removeClass('active');
                    var path = url.split('?')[0];
                    $('.sidebar .nav-link[href="' + path + '"]').addClass('active');

                    // Cambiar URL en el historial del navegador
                    if (pushState !== false) {
                        history.pushState(null, '', url);
                    }

                    // Cambiar título en la topbar de forma dinámica
                    var nuevoTitulo = 'Dashboard';
                    if (path.toLowerCase().indexOf('ordencompra') >= 0) {
                        nuevoTitulo = 'Órdenes de Compra';
                        if (path.toLowerCase().indexOf('detail') >= 0) nuevoTitulo = 'Detalle de OC';
                        if (path.toLowerCase().indexOf('edit') >= 0) nuevoTitulo = 'Editar OC';
                        if (path.toLowerCase().indexOf('create') >= 0) nuevoTitulo = 'Nueva OC';
                    }
                    $('.topbar .topbar-title').text(nuevoTitulo);

                    reinicializarPlugins();

                    // Ejecutar scripts específicos de la vista cargada
                    var scripts = doc.querySelectorAll('script');
                    scripts.forEach(function (script) {
                        var src = script.getAttribute('src') || '';
                        // Evitar recargar librerías globales principales
                        if (!src || (src.indexOf('bootstrap') === -1 &&
                                     src.indexOf('jquery') === -1 &&
                                     src.indexOf('select2') === -1 &&
                                     src.indexOf('site.js') === -1)) {
                            if (src) {
                                // Script externo: crear elemento y agregar — el browser lo descarga y ejecuta
                                var extScript = document.createElement('script');
                                extScript.src = src;
                                document.body.appendChild(extScript);
                                // NO remover: puede interrumpir la descarga en algunos browsers
                            } else {
                                // Script inline: usar eval() para garantizar ejecución síncrona
                                try {
                                    eval(script.textContent);
                                } catch(e) {
                                    console.error('SPA script error:', e);
                                }
                            }
                        }
                    });
                },
                error: function () {
                    container.css('opacity', '1');
                    if (pushState !== false) {
                        window.location.href = url; // fallback tradicional
                    } else {
                        window.location.reload();
                    }
                }
            });
        }

        // Función para refrescar la página SPA actual sin alterar el historial
        function recargarPaginaSPA() {
            cargarPaginaSPA(window.location.pathname + window.location.search, false);
        }

        // 1. Navegación SPA por AJAX para Enlaces Internos (Menú lateral y tablas)
        $(document).on('click', 'a[href]', function (e) {
            var url = $(this).attr('href');

            // Exclusiones: enlaces vacíos, descargas, hojas de ruta, modals o externos
            if (!url || url === '#' || url.startsWith('javascript:') || url.startsWith('#') ||
                $(this).attr('target') === '_blank' || $(this).hasClass('btn-close') ||
                url.toLowerCase().indexOf('descargarpdf') >= 0 ||
                url.toLowerCase().indexOf('exportarpdf') >= 0 ||
                url.toLowerCase().indexOf('generarhojaruta') >= 0) {
                return;
            }

            e.preventDefault();
            // - [x] Refactorizar cargarPaginaSPA en site.js para extraer el main-content y ejecutar los scripts locales
            // - [x] Simplificar el controlador onpopstate en site.js usando la nueva función unificada
            // - [x] Corregir la pérdida de productos al guardar (model binding no contiguo con Detalles.Index) en templates
            // - [x] Implementar delegación de eventos con jQuery y envoltura document ready en Edit.cshtml para total estabilidad
            cargarPaginaSPA(url, true);
        });

        // 2. Control del botón de retroceso/adelante del navegador (SPA)
        window.onpopstate = function () {
            cargarPaginaSPA(window.location.pathname + window.location.search, false);
        };

        // 3. Envío de filtros por AJAX unificado
        $(document).on('submit', '.frm-filtros-ajax', function (e) {
            e.preventDefault();
            var $form = $(this);
            var target = $form.attr('data-target') || '#oc-table-container';
            var container = $(target);
            container.css('opacity', '0.6');

            $.ajax({
                url: $form.attr('action') || window.location.pathname,
                type: 'GET',
                headers: { "X-Requested-With-Partial": "true" },
                data: $form.serialize(),
                success: function (html) {
                    container.html(html).css('opacity', '1');
                },
                error: function () {
                    container.css('opacity', '1');
                    alert('Error al realizar la búsqueda.');
                }
            });
        });

        // 5. Botón Limpiar filtros (AJAX)
        $(document).on('click', '.btn-limpiar-filtros', function () {
            var $form = $(this).closest('form');
            $form[0].reset();
            $form.find('select').val('').trigger('change');
            $form.submit();
        });

        // 6. Modal Nueva OC - GET Form / Restore
        $(document).on('click', '.btn-nueva-oc', function (e) {
            e.preventDefault();
            var modalEl = document.getElementById('nuevaOcModal');
            var modal = bootstrap.Modal.getOrCreateInstance(modalEl);

            if (isModalDraft) {
                // Restaurar borrador existente sin llamar al servidor
                $('#modalDraftDock').fadeOut(200);
                modal.show();
                return;
            }

            modal.show();

            $('#nuevaOcModalBody').html(
                '<div class="text-center py-5">' +
                '  <div class="spinner-border text-primary" role="status">' +
                '    <span class="visually-hidden">Cargando formulario...</span>' +
                '  </div>' +
                '</div>'
            );

            $.ajax({
                url: '/OrdenCompra/Create',
                type: 'GET',
                success: function (html) {
                    $('#nuevaOcModalBody').html(html);
                    inicializarFormularioModal();
                },
                error: function () {
                    $('#nuevaOcModalBody').html('<div class="alert alert-danger m-3">Error al cargar el formulario.</div>');
                }
            });
        });

        // 6b. Minimizar modal a dock
        $(document).on('click', '#btnMinimizarModal', function (e) {
            e.preventDefault();
            var ocNum = $('#NumeroOC').val()?.trim() || 'Nueva OC';
            $('#draftOcNumber').text(ocNum);

            var modalEl = document.getElementById('nuevaOcModal');
            var modal = bootstrap.Modal.getOrCreateInstance(modalEl);

            modalEl.classList.add('minimizing-draft');
            modal.hide();

            $('#modalDraftDock').fadeIn(250);
            isModalDraft = true;
            modalEl.classList.remove('minimizing-draft');
        });

        // 6c. Restaurar borrador desde Dock
        $(document).on('click', '#dockHeader, #btnRestoreDraft', function (e) {
            if ($(e.target).closest('#btnDiscardDraft').length > 0) return;
            e.preventDefault();

            $('#modalDraftDock').fadeOut(200);

            var modalEl = document.getElementById('nuevaOcModal');
            var modal = bootstrap.Modal.getOrCreateInstance(modalEl);
            modal.show();
        });

        // 6d. Descartar borrador desde Dock
        $(document).on('click', '#btnDiscardDraft', function (e) {
            e.preventDefault();
            if (confirm('¿Está seguro de descartar este borrador? Se perderán los datos.')) {
                descartarBorrador();
            }
        });

        // Escuchar cuando el modal se oculta
        $('#nuevaOcModal').on('hide.bs.modal', function (e) {
            var modalEl = this;
            if (!modalEl.classList.contains('minimizing-draft')) {
                descartarBorrador();
            }
        });

        function descartarBorrador() {
            $('#modalDraftDock').hide();
            $('#nuevaOcModalBody').html(
                '<div class="text-center py-5">' +
                '  <div class="spinner-border text-primary" role="status">' +
                '    <span class="visually-hidden">Cargando...</span>' +
                '  </div>' +
                '</div>'
            );
            isModalDraft = false;
        }

        // 7. Submit Form Nueva OC / Editar OC (POST Form AJAX)
        $(document).on('submit', '#frmOC', function (e) {
            e.preventDefault();
            var $form = $(this);
            var formData = new FormData(this);

            var $btnSubmit = $form.find('.btn-submit-oc-form, button[type="submit"]');
            var originalHtml = $btnSubmit.html();
            $btnSubmit.prop('disabled', true).html('<span class="spinner-border spinner-border-sm me-1" role="status" aria-hidden="true"></span> Guardando...');

            $.ajax({
                url: $form.attr('action'),
                type: 'POST',
                data: formData,
                processData: false,
                contentType: false,
                success: function (res) {
                    if (res.success) {
                        isModalDraft = false;
                        $('#modalDraftDock').hide();

                        var modalEl = document.getElementById('nuevaOcModal');
                        if (modalEl) {
                            var modal = bootstrap.Modal.getOrCreateInstance(modalEl);
                            modal.hide();
                        }

                        // Mostrar Toast de éxito
                        var toast = $(
                            '<div class="alert alert-success alert-dismissible fade show shadow-lg" style="position:fixed; top:24px; right:24px; z-index:99999;" role="alert">' +
                            '  <i class="bi bi-check-circle-fill me-2"></i>' + res.message +
                            '  <button type="button" class="btn-close" data-bs-dismiss="alert"></button>' +
                            '</div>'
                        );
                        $('body').append(toast);
                        setTimeout(function () {
                            toast.fadeOut(function () { $(this).remove(); });
                        }, 4000);

                        // Si hay redirección (ej. desde Edición), navegar usando SPA
                        if (res.redirectUrl) {
                            cargarPaginaSPA(res.redirectUrl, true);
                        } else {
                            // Recargar tabla activa mediante submit de filtros
                            var $formFiltros = $('.frm-filtros-ajax');
                            if ($formFiltros.length > 0) {
                                $formFiltros.submit();
                            } else {
                                recargarPaginaSPA();
                            }
                        }
                    } else {
                        // En caso de fallas de validación
                        if ($('#nuevaOcModal').is(':visible')) {
                            $('#nuevaOcModalBody').html(res);
                            inicializarFormularioModal();
                        } else {
                            // Si estamos en la página completa Edit.cshtml
                            var $newContent = $(res).find('.content-area').length > 0 
                                ? $(res).find('.content-area').html() 
                                : res;
                            $('.content-area').html($newContent);
                            reinicializarPlugins();
                        }
                        $btnSubmit.prop('disabled', false).html(originalHtml);
                    }
                },
                error: function () {
                    alert('Error al procesar la solicitud.');
                    $btnSubmit.prop('disabled', false).html(originalHtml);
                }
            });
        });

        // Helper para inicializar dinámicos en el modal
        function inicializarFormularioModal() {
            // Inicializar Select2 en modal
            $('#nuevaOcModalBody .select2-simple').select2({
                dropdownParent: $('#nuevaOcModal'),
                placeholder: 'Seleccionar...',
                allowClear: true,
                width: '100%'
            });
            $('#nuevaOcModalBody select:not(.select2-simple):not(.estado-producto-select)').select2({
                dropdownParent: $('#nuevaOcModal'),
                width: '100%'
            });

            // Inicializar Autocomplete Entidades
            $('#entidadInput').autocomplete({
                source: function (req, resp) {
                    $.getJSON('/OrdenCompra/AutocompleteEntidades', { term: req.term }, resp);
                },
                minLength: 2
            });

            // Inicializar Autocomplete Departamentos de Perú
            const departamentosPeru = [
                "Amazonas", "Áncash", "Apurímac", "Arequipa", "Ayacucho", "Cajamarca", "Callao", "Cusco",
                "Huancavelica", "Huánuco", "Ica", "Junín", "La Libertad", "Lambayeque", "Lima", "Loreto",
                "Madre de Dios", "Moquegua", "Pasco", "Piura", "Puno", "San Martín", "Tacna", "Tumbes", "Ucayali"
            ];
            $('#departamentoInput').autocomplete({
                source: departamentosPeru,
                minLength: 0
            }).on('focus', function () {
                $(this).autocomplete('search', '');
            });

            // Lógica de agregar/quitar productos en la grilla del modal
            var $body = $('#productosBody');
            var $tmpl = $('#tmplProducto');
            var estadosOpts = $('#estadosOptionsData').data('options') || '';
            var idx = 0;

            function addProdRow(datos = null) {
                var html = $tmpl.html()
                    .replaceAll('__IDX__', idx)
                    .replace('ESTADOS_OPTIONS', estadosOpts);

                var $row = $(html);
                if (datos) {
                    $row.find('[name$=".CodigoProducto"]').val(datos.codigo || '');
                    $row.find('[name$=".Descripcion"]').val(datos.descripcion || '');
                    $row.find('[name$=".Cantidad"]').val(datos.cantidad || 1);
                    $row.find('[name$=".EstadoProductoId"]').val(datos.estadoId || 1);
                    $row.find('[name$=".DetalleId"]').val(datos.detalleId || 0);
                    if (datos.motivo) {
                        $row.find('[name$=".MotivoEntregaParcial"]').val(datos.motivo);
                        $row.find('.motivo-parcial').show();
                    }
                }
                $body.append($row);

                // Listener de estado
                $row.find('.estado-producto-select').on('change', function () {
                    var text = $(this).find('option:selected').text();
                    $row.find('.motivo-parcial').toggle(text === 'Entrega Parcial');
                });
                idx++;
            }

            $('#btnAgregarProducto').off('click').on('click', function () {
                addProdRow();
            });

            $body.off('click', '.btn-eliminar-producto').on('click', '.btn-eliminar-producto', function () {
                $(this).closest('tr').remove();
            });

            // Inicializar con 1 producto vacío
            addProdRow();
        }

    });
}
