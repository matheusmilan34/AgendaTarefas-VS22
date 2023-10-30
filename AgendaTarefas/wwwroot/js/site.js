function ConfirmarExclusao(tarefaId, nomeTarefa) {
    $(".nomeTarefa").text(nomeTarefa);
    $(".confirm-excluir.modal").modal();
    $(".btnExcluir").on('click', function () {
        $.ajax({
            url: 'Tarefas/ExcluirTarefa',
            method: 'POST',
            data: { tarefaId: tarefaId },
            success: function (data) {
                $(".modal").modal('hide');
                location.reload(true);
            }
        });
    });
    $('.btnFechar').on('click', function () {
        tarefaId = null;
        nomeTarefa = null;
        $(".modal").modal('hide');
    });
}

function openTarefaModal(dataTarefa) {
    $.get("/Tarefas/CriarTarefa", { dataTarefa: dataTarefa }, function (data) {
        $("#tarefaModal .modal-body").html(data);
        $("#tarefaModal").modal("show");
    });
}

function openEditarTarefaModal(tarefaId) {
    $.get("/Tarefas/AtualizarTarefa", { tarefaId: tarefaId }, function (data) {
        $("#tarefaEditarModal .modal-body").html(data);
        $("#tarefaEditarModal").modal("show");
    });
}